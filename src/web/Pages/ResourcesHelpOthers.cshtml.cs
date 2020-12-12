using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace web.Pages
{
    public class ResourcesHelpOthersModel : PageModel
    {
        private readonly ILogger<ResourcesHelpOthersModel> _logger;

        public ResourcesHelpOthersModel(
            ILogger<ResourcesHelpOthersModel> logger
        )
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}