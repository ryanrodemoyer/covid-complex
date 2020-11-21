using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace web.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class VuePageModel : PageModel
    {
        private readonly ILogger<VuePageModel> _logger;

        public VuePageModel(ILogger<VuePageModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
