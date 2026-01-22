using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleApp.DbItems;

namespace SampleApp.Pages
{
    public class BrowseModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<SampleUser> _userManager;
        private readonly GameDataStore _gameDataStore;

        public SampleUser CurrentUser { get; set; }


        [BindProperty]
        public Guid? GameId { get; set; }

        public BrowseModel(ILogger<IndexModel> logger, UserManager<SampleUser> userManager, GameDataStore gameDataStore)
        {
            _logger = logger;
            _userManager = userManager;
            _gameDataStore = gameDataStore;
        }

        public IList<GameData> GameItems => CurrentUser != null ? _gameDataStore.GetAllGames().Where(g => g.Status == GameData.GameStatus.WaitingForPlayers).ToList() : [];
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


        public IActionResult OnPostJoinGame()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return RedirectToPage("/User/Login");
            }
            CurrentUser = user;
            if (GameId == null || GameId == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "Invalid Game ID.");
                return Page();
            }
            var game = _gameDataStore.GetById(GameId.Value);
            if (game == null)
            {
                ModelState.AddModelError(string.Empty, "Game not found.");
                return Page();
            }
            try
            {
                game.AddPlayer(CurrentUser.Id);
                _gameDataStore.UpdateGameData(game);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            return RedirectToPage("/Index");
        }
    }
}
