using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace web.Pages
{
    public class ResourcesModel : PageModel
    {
        private readonly ILogger<ResourcesModel> _logger;

        public ResourcesModel(
            ILogger<ResourcesModel> logger
        )
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        //public string FlashMessage { get; set; }

        //public IActionResult OnPost()
        //{
        //    return new PageResult();
        //}
    }
}