using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace web.Pages
{
    public class ResourcesComorbiditiesModel : PageModel
    {
        private readonly ILogger<ResourcesComorbiditiesModel> _logger;

        public ResourcesComorbiditiesModel(
            ILogger<ResourcesComorbiditiesModel> logger
        )
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}