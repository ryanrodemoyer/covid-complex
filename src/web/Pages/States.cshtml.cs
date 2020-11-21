using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using web.Data;

namespace web.Pages
{
    public class StatesModel : PageModel
    {
        private readonly ILogger<StatesModel> _logger;
        private readonly ApplicationDbContext _context;

        public StatesModel(ILogger<StatesModel> logger
            , ApplicationDbContext context
        )
        {
            _logger = logger;
            _context = context;
        }

        public string FlashMessage { get; set; }

        public List<State> States { get; set; }

        public async Task OnGet()
        {
            bool shouldHydrate = MemoryCache.AllStates == null;
            if (shouldHydrate)
            {
                List<State> states = await _context.States.OrderBy(x => x.StateName).ToListAsync();

                MemoryCache.AllStates = states;
                States = states;
            }
            else
            {
                States = MemoryCache.AllStates;
            }
        }
    }
}
