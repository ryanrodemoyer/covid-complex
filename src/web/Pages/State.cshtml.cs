using System;
using System.Collections.Concurrent;
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
    public class StateModel : PageModel
    {
        private readonly ILogger<StateModel> _logger;
        private readonly ApplicationDbContext _context;

        public StateModel(
            ILogger<StateModel> logger
            , ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string State { get; set; }

        public string FlashMessage { get; set; }

        public State TargetState { get; set; }

        public async Task OnGet()
        {
            MemoryCache.CachedWhen.TryGetValue("StatesCache", out DateTime cachedAt);

            TimeSpan diff = DateTime.Now - cachedAt;
            if (diff.Hours >= 1)
            {
                MemoryCache.CachedWhen.TryAdd("StatesCache", DateTime.Now);
                MemoryCache.StatesCache.Clear();
            }

            bool exists = MemoryCache.StatesCache.ContainsKey(State);
            if (exists)
            {
                TargetState = MemoryCache.StatesCache[State];
            }
            else
            {
                State state = await (_context.States
                        .Include(x => x.Counties)
                            .ThenInclude(y => y.Records)
                        .FirstOrDefaultAsync(x => x.StateName.ToLower() == State.Replace("-", " "))
                    );

                if (state == null)
                {
                    FlashMessage = "State and county combination do not exist.";
                }
                else
                {
                    state.Counties = state.Counties.OrderByDescending(x => x.Population).ThenBy(x => x.CountyName).ToList();

                    bool added = MemoryCache.StatesCache.TryAdd(State, state);
                    if (added)
                    {
                        _logger.LogInformation($"Successfully cached State={State}");
                    }
                    else
                    {
                        _logger.LogInformation($"Already exists State={State}");
                    }

                    TargetState = state;
                }
            }
        }
    }

    public static class MemoryCache
    {
        public static ConcurrentDictionary<string, DateTime> CachedWhen = new ConcurrentDictionary<string, DateTime>();

        public static ConcurrentDictionary<string, State> StatesCache => new ConcurrentDictionary<string, State>();

        public static List<State> AllStates = null;
    }
}
