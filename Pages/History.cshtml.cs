using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleApp.DbItems;

namespace SampleApp.Pages
{
    public class HistoryModel : PageModel
    {

        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<SampleUser> _userManager;
        private readonly GameDataStore _gameDataStore;

        public SampleUser CurrentUser { get; set; }

        public HistoryModel(ILogger<IndexModel> logger, UserManager<SampleUser> userManager, GameDataStore gameDataStore)
        {
            _logger = logger;
            _userManager = userManager;
            _gameDataStore = gameDataStore;
        }

        public IList<GameData> GameItems => CurrentUser != null ? _gameDataStore.GetAllByPlayer(CurrentUser.Id).ToList() : [];
        public IActionResult OnGet()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return RedirectToPage("/User/Login");
            }
            CurrentUser = user;
            return Page();
        }
    }
}
