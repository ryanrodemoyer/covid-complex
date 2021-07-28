using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using web.Data;

namespace web
{
    public interface IDataSetRetriever
    {
        public string GetFilename();
    }

    public class CovidDataUpdater
    {
        private readonly ILogger<CovidDataUpdater> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IDataSetRetriever _retriever;

        public CovidDataUpdater(
            ILogger<CovidDataUpdater> logger
            , ApplicationDbContext context
            , IDataSetRetriever retriever)
        {
            _logger = logger;
            _context = context;
            _retriever = retriever;
        }

        public void Update(CancellationToken cancellationToken)
        {
            string GetGitHubApiData()
            {
                try
                {
                    _logger.LogInformation("calling github");

                    string uri = "https://api.github.com/repos/nytimes/covid-19-data/commits?path=us-counties.csv";

                    using var http = new HttpClient();

                    http.DefaultRequestHeaders.Add("User-Agent", "covid-complex-bot");

                    HttpResponseMessage res = http.GetAsync(uri).Result;

                    _logger.LogInformation($"HTTP Response = {(int)res.StatusCode}");

                    string json = res.Content.ReadAsStringAsync().Result;
                    return json;
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error occurred during GitHub api call.{Environment.NewLine}{e.Message}");

                    return null;
                }
            }

            string json = GetGitHubApiData();

            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogInformation("problem getting data from api");
            }
            else
            {
                var jarr = JArray.Parse(json);

                if (jarr.Count == 0)
                {
                    _logger.LogInformation("response contain no records");
                }
                else
                {
                    // data returned from GitHub is already sorted by most recent commit

                    dynamic mostRecent = jarr
                        .Select(x =>
                            new
                            {
                                sha = x.SelectToken("sha")
                                , date = DateTime.Parse(x.SelectToken("commit.author.date").Value<string>())
                            })
                        .First();

                    SiteSettings settings = _context.SiteSettings.FirstOrDefault();
                    if (settings == null)
                    {
                        settings = new SiteSettings();
                        settings.LastUpdatedWhen = DateTime.UtcNow;
                        _context.SiteSettings.Add(settings);
                    }

                    _logger.LogInformation($"app is at {settings.USCountiesSha ?? "<empty>"}, GitHub is at sha={mostRecent.sha}|date={mostRecent.date}");

                    bool missing = string.IsNullOrWhiteSpace(settings.USCountiesSha);
                    if (missing)
                    {
                        _logger.LogInformation($"SiteSettings.USCountiesSha is empty. Updating with {mostRecent.sha}");

                        settings.USCountiesSha = mostRecent.sha;
                        _context.SaveChanges();

                        _logger.LogInformation("skipped");

                    }
                    else if (mostRecent.sha != settings.USCountiesSha)
                    {
                        _logger.LogInformation($"updating");

                        settings.LastUpdatedWhen = DateTime.UtcNow;
                        settings.USCountiesSha = mostRecent.sha;

                        ReadAllStatesAndCounties(cancellationToken);
                    }
                }
            }
        }

        private void ReadAllStatesAndCounties(CancellationToken cancellationToken)
        {
            Dictionary<string, State> states = new DataSetFileReader(_retriever).Read(DateTime.Now.AddDays(-45));

            foreach (State state in states.Values)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"{DateTimeOffset.Now} cancellation requested. Exiting.");

                    return;
                }

                foreach (County county in state.Counties)
                {
                    List<CovidDayStaging> records =
                        county.Records.Select(z =>
                            new CovidDayStaging
                            {
                                Date = z.Date
                                , Cases = z.Cases
                                , Deaths = z.Deaths
                            }).ToList();

                    try
                    {
                        County c = _context.Counties.Single(x => x.CountyName == county.CountyName && x.State.StateName == state.StateName);
                        c.RecordsStaging.AddRange(records);
                    }
                    catch (InvalidOperationException e)
                    {
                        _logger.LogError($"{DateTimeOffset.Now} error during county lookup. State={state.StateName}, County={county.CountyName}");
                    }
                }

                _logger.LogInformation($"{DateTimeOffset.Now} saving changes to CovidDaysStaging->{state.StateName}");
                _context.SaveChanges();
                _logger.LogInformation($"{DateTimeOffset.Now} saved!");
            }

            // this bulk save mechanism was causing an out of memory exception within the container
            //_logger.LogInformation($"{DateTimeOffset.Now} saving changes to CovidDaysStaging");
            //_context.Database.SetCommandTimeout(4 * 60); // 4 minutes
            //_context.SaveChanges();
            //_logger.LogInformation($"{DateTimeOffset.Now} saved changes");

            // move net-new records from the staging table (CovidDaysStaging) to the live table (CovidDays)
            string sql_InsertRecords = @"
		        insert into CovidDays (CountyId,Date,Cases,Deaths)
		        select stg.CountyId,stg.Date,stg.Cases,stg.Deaths from CovidDaysStaging stg
		        left join CovidDays curr on stg.CountyId=curr.CountyId and stg.Date=curr.Date
		        where curr.Id is null
	        ";

            // todo needs work, not ready
            string sql_UpdateRecords = @"
                update CovidDays set 
                    Cases=stg.Cases,
                    Deaths=stg.Deaths,
                from 
                    (
                        select CountyId,Date,Cases,Deaths
                        from CovidDaysStaging
                    ) as stg
                where stg.CountyId=CovidDays.CountyId
                        and stg.Date=CovidDays.Date
                ";

            // find records where the current and staging tables have different values for cases or deaths
            string sql_NeedsUpdated = @"
                select count(*)
                from 
	                CovidDays curr
	                inner join CovidDaysStaging stg on curr.CountyId=stg.CountyId and curr.Date=stg.Date
                WHERE
	                stg.Cases<>curr.Cases or stg.Deaths<>curr.Deaths
                ";

            string sql_DeleteStaging = "delete from CovidDaysStaging";

            _context.Database.GetDbConnection().Open();

            //using var cmdUpdate = _context.Database.GetDbConnection().CreateCommand();
            //cmdUpdate.CommandText = sql_UpdateRecords;
            //int updated = cmdUpdate.ExecuteNonQuery();
            //_logger.LogInformation($"Update processed {updated} records.");

            using var cmdNeedsUpdated = _context.Database.GetDbConnection().CreateCommand();
            cmdNeedsUpdated.CommandText = sql_NeedsUpdated;
            int needsUpdated = cmdNeedsUpdated.ExecuteNonQuery();
            _logger.LogInformation($"Found {needsUpdated} records that need updated.");

            _logger.LogInformation($"{DateTimeOffset.Now}: Inserting records...");
            using var cmdInsert = _context.Database.GetDbConnection().CreateCommand();
            cmdInsert.CommandText = sql_InsertRecords;
            int inserted = cmdInsert.ExecuteNonQuery();
            _logger.LogInformation($"{DateTimeOffset.Now}: Insert processed {inserted} records.");

            _logger.LogInformation($"{DateTimeOffset.Now}: Deleting records...");
            using var cmd2 = _context.Database.GetDbConnection().CreateCommand();
            cmd2.CommandText = sql_DeleteStaging;
            int deleted = cmd2.ExecuteNonQuery();
            _logger.LogInformation($"{DateTimeOffset.Now}: Removed {deleted} records from CovidDaysStaging.");


            _context.Database.GetDbConnection().Close();
        }
    }

    public class DataSetFileReader
    {
        private readonly IDataSetRetriever _dataSetRetriever;

        public DataSetFileReader(IDataSetRetriever dataSetRetriever)
        {
            _dataSetRetriever = dataSetRetriever;
        }

        public Dictionary<string, State> Read(DateTime? onOrAfter = null)
        {
            string filename = _dataSetRetriever.GetFilename();

            try
            {
                var results = new List<CovidRow>();

                using var file = new StreamReader(filename);
                string line = file.ReadLine();

                while ((line = file.ReadLine()) != null)
                {
                    var row = CovidRow.FromCsv(line);

                    if (onOrAfter.HasValue)
                    {
                        bool check = row.Date.Date < onOrAfter.Value.Date;
                        if (check)
                        {
                            continue;
                        }
                    }

                    results.Add(row);
                }

                var states = new Dictionary<string, State>();

                foreach (var record in results)
                {
                    bool exists = states.TryGetValue(record.State, out var counties);
                    if (exists)
                    {
                        State state = states[record.State];

                        bool cExists = state.Counties.Exists(x => x.Fips == record.Fips);
                        if (cExists)
                        {
                            var c = state.Counties.Single(x => x.Fips == record.Fips);
                            c.Records.Add(new CovidDay { Date = record.Date, Cases = record.Cases, Deaths = record.Deaths });
                        }
                        else
                        {
                            var county = new County { CountyName = record.County, Fips = record.Fips };
                            county.Records.Add(new CovidDay { Date = record.Date, Cases = record.Cases, Deaths = record.Deaths });

                            state.Counties.Add(county);
                        }
                    }
                    else
                    {
                        var county = new County { CountyName = record.County, Fips = record.Fips };
                        county.Records.Add(new CovidDay { Date = record.Date, Cases = record.Cases, Deaths = record.Deaths });

                        var state = new State { StateName = record.State };
                        state.Counties.Add(county);

                        states.Add(record.State, state);
                    }
                }

                return states;
            }
            finally
            {
                try
                {
                    Console.WriteLine("Deleting file...");
                    File.Delete(filename);
                    Console.WriteLine("Deleted file.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error deleting file. Message={e.Message}");
                }
            }
        }
    }

    public class GitHubDataSetRetriever : IDataSetRetriever
    {
        private readonly ILogger<GitHubDataSetRetriever> _logger;

        public GitHubDataSetRetriever(
            ILogger<GitHubDataSetRetriever> logger
            )
        {
            _logger = logger;
        }

        public string GetFilename()
        {
            try
            {
                _logger.LogInformation("calling github");

                string uri = "https://github.com/nytimes/covid-19-data/raw/master/us-counties.csv";

                using var http = new HttpClient();

                http.DefaultRequestHeaders.Add("User-Agent", "covid-complex-bot");

                HttpResponseMessage res = http.GetAsync(uri).Result;

                _logger.LogInformation($"HTTP Response = {(int)res.StatusCode}");

                string json = res.Content.ReadAsStringAsync().Result;

                string filename = Path.GetTempFileName();
                _logger.LogInformation($"Writing us-counties.csv data to {filename}");

                File.WriteAllText(filename, json);
                _logger.LogInformation($"Save to disk successful.");

                return filename;
            }
            catch (Exception e)
            {
                _logger.LogInformation("Error occurred during GitHub api call.");
                _logger.LogInformation(e.Message);

                return null;
            }
        }
    }

    public class CovidRow
    {
        public DateTime Date { get; set; }
        public string State { get; set; }
        public string County { get; set; }
        public int Fips { get; set; }
        public int Cases { get; set; }
        public int Deaths { get; set; }

        public static CovidRow FromCsv(string line)
        {
            string[] cols = line.Split(",");

            try
            {
                if (cols.Length == 6)
                {
                    string fips = cols[3];

                    string deaths = string.IsNullOrWhiteSpace(cols[5]) ? "0" : cols[5];

                    // county
                    var row = new CovidRow();
                    row.Date = DateTime.Parse(cols[0]);
                    row.County = cols[1];
                    row.State = cols[2];
                    row.Fips = string.IsNullOrWhiteSpace(fips) ? 0 : Convert.ToInt32(fips);
                    row.Cases = Convert.ToInt32(cols[4]);
                    row.Deaths = Convert.ToInt32(deaths);

                    return row;
                }
                else
                {
                    string fips = cols[3];

                    var row = new CovidRow();
                    row.Date = DateTime.Parse(cols[0]);
                    row.State = cols[1];
                    row.Fips = string.IsNullOrWhiteSpace(fips) ? 0 : Convert.ToInt32(fips);
                    row.Cases = Convert.ToInt32(cols[3]);
                    row.Deaths = Convert.ToInt32(cols[4]);

                    return row;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CovidRow.FromCSV: EXCEPTION");
                Console.WriteLine(line);
                Console.WriteLine($"Exception message={e.Message}");

                throw;
            }
        }
    }
}
