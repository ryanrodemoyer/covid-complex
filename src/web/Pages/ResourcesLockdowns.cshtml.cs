using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace web.Pages
{
    public class ResourcesLockdownsModel : PageModel
    {
        private readonly ILogger<ResourcesLockdownsModel> _logger;

        public ResourcesLockdownsModel(
            ILogger<ResourcesLockdownsModel> logger
        )
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}