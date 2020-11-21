using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using web.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddControllers();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));


            services.AddScoped<IDataSetRetriever, GitHubDataSetRetriever>();
            services.AddScoped<CovidDataUpdater>();
            services.AddHostedService<TimedDataUpdater>();

            IMvcBuilder builder = services.AddRazorPages();

#if DEBUG
            builder.AddRazorRuntimeCompilation();
#endif
            
            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // forward headers - begin
            // https://stackoverflow.com/questions/43860128/asp-net-core-google-authentication/
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto, RequireHeaderSymmetry = false
            };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardedHeadersOptions);
            // forward headers - end

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();

                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // enables the use of API controllers in the project
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();

                // endpoints.MapFallbackToPage("/Index");
                // endpoints.MapFallbackToController("Index", "Home");
            });
        }
    }

    public class TimedDataUpdater : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<TimedDataUpdater> _logger;
        
        private Timer _timer;

        private CancellationToken _eject;

        public TimedDataUpdater(
            IServiceProvider services
            , ILogger<TimedDataUpdater> logger
            )
        {
            _services = services;
            _logger = logger;
        }

        private void callback(object state)
        {
            _logger.LogInformation($"{DateTimeOffset.Now}: elapsed");

            dynamic d = (dynamic) state;

            using var scope = _services.CreateScope();

            var scopedProcessingService =
                scope.ServiceProvider
                    .GetRequiredService<CovidDataUpdater>();

            scopedProcessingService.Update(d.cts);

            _logger.LogInformation($"{DateTimeOffset.Now}: sleeping");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service running.");

            _eject = stoppingToken;

            await DoWork(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);

            _logger.LogInformation($"{DateTimeOffset.Now} stoppingToken.IsCancellationRequested={_eject.IsCancellationRequested}");

        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service is working.");

            _timer = new Timer(callback, new {cts = stoppingToken}, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(60));
        }

        public override void Dispose()
        {
            base.Dispose();

            _timer?.Dispose();
        }
    }
}