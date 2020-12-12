using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace web.Pages
{
    public class SourceCodeModel : PageModel
    {
        private readonly ILogger<SourceCodeModel> _logger;

        public SourceCodeModel(
            ILogger<SourceCodeModel> logger
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