using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleApp.DbItems;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Pages.User
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<SampleUser> _signInManager;

        public LoginModel(SignInManager<SampleUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public LoginInput Login { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class LoginInput
        {
            [Required]
            public string UserName { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }


        public async Task<IActionResult> OnPostLoginAsync()
        {
            ModelState.Clear();
            TryValidateModel(Login, nameof(Login));
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                Login.UserName,
                Login.Password,
                Login.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
                return Redirect("/");

            ErrorMessage = "Invalid login attempt.";
            return Page();
        }

    }

}