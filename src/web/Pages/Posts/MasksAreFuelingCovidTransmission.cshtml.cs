using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace web.Pages
{
    public class MasksAreFuelingCovidTransmissionModel : PageModel
    {
        private readonly ILogger<MasksAreFuelingCovidTransmissionModel> _logger;

        public MasksAreFuelingCovidTransmissionModel(
            ILogger<MasksAreFuelingCovidTransmissionModel> logger
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