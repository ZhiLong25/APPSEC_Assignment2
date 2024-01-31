using APPSEC_Assignment2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace APPSEC_Assignment2.Pages
{

    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<Register> _userManager;
        private readonly SignInManager<Register> _signInManager;

        public ChangePasswordModel(UserManager<Register> userManager, SignInManager<Register> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [BindProperty]
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            return Page();

        }


        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var timeSinceLastChange = DateTime.UtcNow - user.PasswordChangedDate;

            if (timeSinceLastChange < TimeSpan.FromMinutes(1))
            {
                ModelState.AddModelError(string.Empty, "Cannot change password within 1 minutes of the last change.");
                return Page();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            user.PasswordChangedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage("/Index"); // Redirect to home page or another page after successful password change.
        }
    }
}
