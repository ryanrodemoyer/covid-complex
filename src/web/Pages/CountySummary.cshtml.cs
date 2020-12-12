using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using web.Data;

namespace web.Pages
{
    [AllowAnonymous]
    public class CountySummaryModel : PageModel
    {
        private readonly ILogger<CountySummaryModel> _logger;
        private readonly ApplicationDbContext _context;

        public CountySummaryModel(
            ILogger<CountySummaryModel> logger
            , ApplicationDbContext context
        )
        {
            _logger = logger;
            _context = context;
        }

        public string FlashMessage { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string State { get; set; }

        [BindProperty(SupportsGet = true)]
        public string County { get; set; }

        public County TargetCounty { get; set; }
        public CountyAnalysis Analysis { get; set; }
        public SiteSettings SiteSettings { get; set; }

        public async Task OnGet()
        {
            string countySafe = County.Replace("-", " ").ToLower();
            string stateSafe = State.Replace("-", " ").ToLower();

            County county = await _context.Counties
                .Include(x => x.State)
                .Include(x => x.Records)
                .FirstOrDefaultAsync(x => x.CountyName.ToLower() == countySafe && x.State.StateName.ToLower() == stateSafe);

            if (county == null)
            {
                FlashMessage = $"State and county combination do not exist.";
            }
            else
            {
                county.Records = county.Records.OrderBy(x => x.Date).ToList();

                SiteSettings settings = await _context.SiteSettings.FirstAsync();

                var analysis = CountyProcessor.CountyProcess(county.Records);

                TargetCounty = county;
                Analysis = analysis;
                SiteSettings = settings;
            }
        }

        //public Task<IActionResult> OnPost()
        //{

        //}
    }


    public static class CountyProcessor
    {
        public static CountyAnalysis CountyProcess(List<CovidDay> rows)
        {
            CovidDay previous = null;

            var results = new List<AnalysisRow>();

            foreach (var row in rows)
            {
                var r = new AnalysisRow();
                r.Date = row.Date;
                r.State = row.County.State.StateName;
                r.County = row.County.CountyName;
                r.CumulitiveCases = row.Cases;
                r.CumulitiveDeaths = row.Deaths;

                if (previous == null)
                {
                    r.NetNewCases = row.Cases;
                    r.NetNewDeaths = row.Deaths;

                    previous = row;
                }
                else
                {
                    r.NetNewCases = row.Cases - previous.Cases;
                    r.NetNewDeaths = row.Deaths - previous.Deaths;

                    previous = row;
                }

                results.Add(r);
            }

            CovidDay info = rows.Last();
            
            // loop the 14 most recent days
            // calculate the average new cases per day for each of the past 14 days
            int count = 0;
            for (int i = results.Count - 1; ++count <= 14; i--)
            {
                int sum = 0;

                int innerCount = 0;
                for (int j = i; ++innerCount <= 14; j--)
                {
                    sum += results[j].NetNewCases;
                }

                int avg = sum / 14;

                results[i].MovingAvg14DayPer100KCases = Convert.ToInt32(((double)sum / (double)info.County.Population) * 100000);
                results[i].MovingAvg14DayNetNewCases = avg;
            }


            var analysis = new CountyAnalysis(results);
            analysis.State = info.County.State.StateName;
            analysis.County = info.County.CountyName;
            analysis.CumulitiveCases = info.Cases;
            analysis.CumulitiveDeaths = info.Deaths;

            if (info.County.Population > 0)
            {
                int population = info.County.Population;
                analysis.Population = population;
                analysis.ResidentsWithoutCovidTotal = population - info.Cases;
                double pct = (double) (population - info.Cases) / (double) population;
                analysis.ResidentsWithoutCovidPercentage = Math.Round(pct, 4) * 100d;
            }

            //analysis.Rows = results;

            return analysis;
        }
    }

    public static class Settings
    {
        public static TimeSpan InfectedWindow = TimeSpan.FromDays(14);

        public static TimeSpan NewCases1Week = TimeSpan.FromDays(7);
        public static TimeSpan NewCases2Week = TimeSpan.FromDays(14);
        public static TimeSpan NewCases3Week = TimeSpan.FromDays(21);
    }

    public class AnalysisRow
    {
        public DateTime Date { get; set; }
        public string State { get; set; }
        public string County { get; set; }

        public int MovingAvg14DayPer100KCases { get; set; }
        public int MovingAvg14DayNetNewCases { get; set; }

        public int CumulitiveCases { get; set; }
        public int NetNewCases { get; set; }

        public int CumulitiveDeaths { get; set; }
        public int NetNewDeaths { get; set; }
    }

	public class CountyAnalysis
	{
        public CountyAnalysis(List<AnalysisRow> results)
        {
            Rows = results;
        }

        public string State { get; set; }
		public string County { get; set; }
		public int? Population { get; set; }
		public int CumulitiveCases { get; set; }
		public int CumulitiveDeaths { get; set; }
		public int ResidentsWithoutCovidTotal { get; set; }
		public double ResidentsWithoutCovidPercentage { get; set; }

        public Dictionary<string, Dictionary<string, double>> StatSummary
		{
			get
			{
				var days = new Dictionary<string, Dictionary<string, double>>();

				for (int i = Rows.Count - 1; i >= Rows.Count - 14; i--)
				{
					var last = Rows[i];
					DateTime now = last.Date;


                    var descending = Rows.OrderByDescending(x => x.Date);
                    IEnumerable<AnalysisRow> recent = descending.Take(14);

                    var caseCount14Days = recent.Sum(x => x.NetNewCases);
					var caseCount14Days14DaysAgo = descending.Skip(13).Take(14).Sum(x => x.NetNewCases);
					var deaths14Days = recent.Sum(x => x.NetNewDeaths);
					var window1Week = Rows.Where(x => x.Date <= now && x.Date > now.Subtract(Settings.NewCases1Week));
					var window2Week = Rows.Where(x => x.Date <= now && x.Date > now.Subtract(Settings.NewCases2Week));
					var window3Week = Rows.Where(x => x.Date <= now && x.Date > now.Subtract(Settings.NewCases3Week));

					double window1WeekAverage = window1Week.Average(x => x.NetNewCases);
					double window2WeekAverage = window2Week.Average(x => x.NetNewCases);
					double window3WeekAverage = window3Week.Average(x => x.NetNewCases);

			        var results = new Dictionary<string, double>();
					results.Add("NetNewCases", last.NetNewCases);
					results.Add("NetNewDeaths", last.NetNewDeaths);
                    
					results.Add("CumulitiveCaseCount14Days", caseCount14Days);
					results.Add("CumulitiveCaseCount14Days14DaysAgo", caseCount14Days14DaysAgo);
					results.Add("CumulitiveDeaths14Days", deaths14Days);
				
                    if (Population > 0)
                    {
                        double per150Factor = (double)Population / 150d;
                        double per100KFactor = (double)Population / 100000d;
                        results.Add("NetNewCasesPer150People", Math.Round((double)last.NetNewCases / per150Factor, 3));

                        results.Add("CumulitiveCaseCount14Days14DaysAgoPercent", Math.Round(((double)caseCount14Days14DaysAgo / (double)Population) * 100, 3));
                        results.Add("CumulitiveCaseCount14Days14DaysAgoPer100K", Convert.ToInt32((double)caseCount14Days14DaysAgo / per100KFactor));

                        results.Add("CumulitiveCaseCount14DaysPercent", Math.Round(((double)caseCount14Days / (double)Population) * 100, 3));
                        results.Add("CumulitiveCaseCount14DaysPer100K", Convert.ToInt32((double)caseCount14Days / per100KFactor));

                        results.Add("DailyNewCases1WeekAvgPer100k", Convert.ToInt32(window1WeekAverage / per100KFactor));
                        results.Add("DailyNewCases2WeekAvgPer100K", Convert.ToInt32(window2WeekAverage / per100KFactor));
                        results.Add("DailyNewCases3WeekAvgPer100K", Convert.ToInt32(window3WeekAverage / per100KFactor));
                    }

					results.Add("DailyNewCases1WeekAverage", (int)window1WeekAverage);
					results.Add("DailyNewCases2WeekAverage", (int)window2WeekAverage);
					results.Add("DailyNewCases3WeekAverage", (int)window3WeekAverage);

				

					results.Add("CumulitiveCaseCount1Week", window1Week.Sum(x => x.NetNewCases));
					results.Add("CumulitiveCaseCount2Week", window2Week.Sum(x => x.NetNewCases));
					results.Add("CumulitiveCaseCount3Week", window3Week.Sum(x => x.NetNewCases));

					days.Add($"{now.Month}/{now.Day}/{now.Year}", results);
				}

				return days;
			}
		}

        public List<AnalysisRow> Rows { get; set; }
	}
}