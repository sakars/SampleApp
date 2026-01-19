using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleApp.DbItems;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Pages.User
{
    public class ProfileModel : PageModel
    {

        private readonly UserManager<SampleUser> _userManager;
        private readonly SignInManager<SampleUser> _signInManager;


        [BindProperty]
        public UserInput UsernameData { get; set; } = new();

        public class UserInput
        {
            [Required]
            public string DisplayName { get; set; } = string.Empty;
        }

        public ProfileModel(UserManager<SampleUser> userManager, SignInManager<SampleUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public void OnGet()
        {
        }

        public async Task OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            user.DisplayName = UsernameData.DisplayName;
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);

            RedirectToPage();
        }

    }
}
