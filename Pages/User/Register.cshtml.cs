using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleApp.DbItems;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Pages
{
    public class RegisterModel : PageModel
    {

        private readonly UserManager<SampleUser> _userManager;
        private readonly SignInManager<SampleUser> _signInManager;

        [BindProperty]
        public RegisterInput Register { get; set; } = new();


        public string? ErrorMessage { get; set; }

        public RegisterModel(UserManager<SampleUser> userManager, SignInManager<SampleUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        public class RegisterInput
        {
            [Required]
            public string UserName { get; set; } = string.Empty;


            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare(nameof(Password))]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Clear();
            TryValidateModel(Register, nameof(Register));
            if (!ModelState.IsValid)
                return Page();

            var user = new SampleUser
            {
                Id = Guid.NewGuid(),
                UserName = Register.UserName,
                DisplayName = Register.UserName
            };

            var result = await _userManager.CreateAsync(user, Register.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect("/");
            }

            ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Page();
        }
    }
}
