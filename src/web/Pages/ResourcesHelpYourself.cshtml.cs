using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace web.Pages
{
    public class ResourcesHelpYourselfModel : PageModel
    {
        private readonly ILogger<ResourcesHelpYourselfModel> _logger;

        public ResourcesHelpYourselfModel(
            ILogger<ResourcesHelpYourselfModel> logger
        )
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}