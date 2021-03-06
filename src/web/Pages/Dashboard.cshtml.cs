﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace web.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;
        private readonly IWebHostEnvironment _environment;

        public DashboardModel(
            ILogger<DashboardModel> logger
            , IWebHostEnvironment environment
            )
        {
            _logger = logger;
            _environment = environment;
        }

        public bool IsDev => _environment.IsDevelopment();

        public void OnGet()
        {
            //var vm = await _viewModelService.GetEntriesForCurrentUserAsync();
            //MyEntries = vm;
            //MyLeaguesJson = JsonConvert.SerializeObject(vm, new JsonSerializerSettings
            //{
            //    ContractResolver = new DefaultContractResolver
            //    {
            //        NamingStrategy = new CamelCaseNamingStrategy()
            //    }
            //});
        }

   
    }
}