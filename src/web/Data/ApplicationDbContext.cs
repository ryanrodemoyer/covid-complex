using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using web.Pages;

namespace web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<State> States { get; set; }
        public DbSet<County> Counties { get; set; }
        public DbSet<CovidDay> Records { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }

        public DbSet<CovidDayStaging> RecordsStaging { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

#if LINQPAD
        // https://linqpad.azureedge.net/public/SampleDbContext.cs

        string _connectionString;

        public ApplicationDbContext()
        {
            string filename = "appsettings.json";
            bool exists = File.Exists(filename);
            if (!exists)
            {
                filename = @"C:\RyanData\Code\covid\ui\src\web\appsettings.json";
            }

            IConfiguration config = new ConfigurationBuilder().AddJsonFile(filename).Build();
            _connectionString = config.GetConnectionString("LinqpadConnection");
        }

        // This property indicates whether or not you're running inside LINQPad:
        internal bool InsideLINQPad => System.AppDomain.CurrentDomain.FriendlyName.StartsWith("LINQPad");
#endif

        static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

#if DEBUG
            optionsBuilder.UseSqlite("DataSource=covid.db");

            //optionsBuilder.UseLoggerFactory(loggerFactory);
            
            ////optionsBuilder.EnableSensitiveDataLogging();
            //optionsBuilder.EnableDetailedErrors();
#endif

#if LINQPAD
            optionsBuilder.UseSqlite(_connectionString);
            //optionsBuilder.UseLoggerFactory(loggerFactory);
            //optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
#endif
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<State>()
                .ToTable("States");

            builder.Entity<County>()
                .ToTable("Counties");

            builder.Entity<CovidDay>()
                .ToTable("CovidDays");


            builder.Entity<CovidDayStaging>()
                .ToTable("CovidDaysStaging");

            builder.Entity<CovidDay>().HasIndex(x => new {CountyId = x.CountyId, Date = x.Date});
        }
    }

    public class SiteSettings
    {
        public int Id { get; set; }

        public DateTime LastUpdatedWhen { get; set; }

        public string USCountiesSha { get; set; }
    }

    public class State
    {
        public int StateId { get; set; }
        public string StateName { get; set; }

        public List<County> Counties { get; set; } = new List<County>();


        [NotMapped]
        public string StateNameForRoute => StateName.ToLower().Replace(" ", "-");
    }

    public class County
    {
        public int StateId { get; set; }

        public int CountyId { get; set; }

        public string CountyName { get; set; }

        public int Fips { get; set; }

        public int Population { get; set; }

        public State State { get; set; }

        public List<CovidDay> Records { get; set; } = new List<CovidDay>();
        public List<CovidDayStaging> RecordsStaging { get; set; } = new List<CovidDayStaging>();
    }

    public class CovidDay
    {
        public int CountyId { get; set; }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Cases { get; set; }
        public int Deaths { get; set; }

        public County County { get; set; }
    }


    public class CovidDayStaging
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Cases { get; set; }
        public int Deaths { get; set; }

        public County County { get; set; }
    }
}