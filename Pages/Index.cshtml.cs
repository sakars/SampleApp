using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleApp.DbItems;

namespace SampleApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<SampleUser> _userManager;

        public SampleUser? CurrentUser { get; set; }

        public IndexModel(ILogger<IndexModel> logger, UserManager<SampleUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public void OnGet()
        {
            CurrentUser = _userManager.GetUserAsync(User).Result;


        }

    }
}
