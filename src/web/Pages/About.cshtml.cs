using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace web.Pages
{
    public class AboutModel : PageModel
    {
        private readonly ILogger<AboutModel> _logger;

        public AboutModel(
            ILogger<AboutModel> logger
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