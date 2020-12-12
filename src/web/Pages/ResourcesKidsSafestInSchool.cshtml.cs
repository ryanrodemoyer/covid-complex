using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace web.Pages
{
    public class ResourcesKidsSafestSchoolModel : PageModel
    {
        private readonly ILogger<ResourcesKidsSafestSchoolModel> _logger;

        public ResourcesKidsSafestSchoolModel(
            ILogger<ResourcesKidsSafestSchoolModel> logger
        )
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}