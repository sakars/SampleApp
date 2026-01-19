using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleApp.DbItems;

namespace ChessOnline.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<SampleUser> _signInManager;

        public LogoutModel(SignInManager<SampleUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }
    }
}
