using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleApp.DbItems;

namespace SampleApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<SampleUser> _userManager;
        private readonly GameDataStore _gameDataStore;

        public SampleUser CurrentUser { get; set; }

        [BindProperty]
        public string? GameName { get; set; }

        [BindProperty]
        public Guid? GameId { get; set; }

        [BindProperty]
        public int Row { get; set; }

        [BindProperty]
        public int Col { get; set; }

        public (int, int, int) WLD => CurrentUser != null ? _gameDataStore.GetWinLoseDraw(CurrentUser.Id) : (0, 0, 0);

        public IndexModel(ILogger<IndexModel> logger, UserManager<SampleUser> userManager, GameDataStore gameDataStore)
        {
            _logger = logger;
            _userManager = userManager;
            _gameDataStore = gameDataStore;
        }

        public IList<GameData> GameItems => CurrentUser != null ? _gameDataStore.GetAllByPlayer(CurrentUser.Id).Where(g => g.Status == GameData.GameStatus.X_Turn || g.Status == GameData.GameStatus.O_Turn).ToList() : [];

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

        public IActionResult OnPostCreateGame()
        {
            if (string.IsNullOrWhiteSpace(GameName))
            {
                ModelState.AddModelError(string.Empty, "Game name cannot be empty.");
                return Page();
            }
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return RedirectToPage("/User/Login");
            }
            CurrentUser = user;
            var newGame = new GameData(GameName);
            newGame.AddPlayer(CurrentUser.Id);

            _gameDataStore.InsertGameData(newGame);

            return RedirectToPage("/Browse");
        }


        public IActionResult OnPostRefresh()
        {
            return RedirectToPage();
        }

        public IActionResult OnPostMakeMove()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                _logger.LogWarning("Unauthenticated user attempted to make a move.");
                return RedirectToPage("/User/Login");
            }
            CurrentUser = user;
            if (GameId == null || GameId == Guid.Empty)
            {
                _logger.LogWarning("User {UserId} provided invalid Game ID to make a move.", CurrentUser.Id);
                ModelState.AddModelError(string.Empty, "Invalid Game ID.");
                return Page();
            }
            var game = _gameDataStore.GetById(GameId.Value);
            if (game == null)
            {
                _logger.LogWarning("User {UserId} attempted to make a move in non-existent Game ID {GameId}.", CurrentUser.Id, GameId);
                ModelState.AddModelError(string.Empty, "Game not found.");
                return Page();
            }
            try
            {
                if (game.PlayerXId == CurrentUser.Id)
                {
                    _logger.LogInformation("User {UserId} is making a move as Player X in Game ID {GameId}.", CurrentUser.Id, GameId);
                    game.PlaceX(Row, Col);
                }
                if (game.PlayerOId == CurrentUser.Id)
                {
                    _logger.LogInformation("User {UserId} is making a move as Player O in Game ID {GameId}.", CurrentUser.Id, GameId);
                    game.PlaceO(Row, Col);
                }
                if (game.PlayerXId != CurrentUser.Id && game.PlayerOId != CurrentUser.Id)
                {
                    _logger.LogWarning("User {UserId} attempted to make a move in Game ID {GameId} but is not a player.", CurrentUser.Id, GameId);
                    ModelState.AddModelError(string.Empty, "You are not a player in this game.");
                    return Page();
                }
                _gameDataStore.UpdateGameData(game);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            return RedirectToPage();
        }
    }
}
