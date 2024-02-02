using APPSEC_Assignment2.Model;
using APPSEC_Assignment2.ViewModel;
using APPSEC_Assignment2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace APPSEC_Assignment2.Pages
{
	[Authorize(Policy = "LoggedIn")]
	[ValidateAntiForgeryToken]
	public class ChangePasswordModel : PageModel
    {

		[BindProperty]
		public ChangePassword CPModel { get; set; }

		private readonly UserManager<Register> _userManager;
        private readonly SignInManager<Register> _signInManager;
		private readonly AuthDbContext _context;

		public ChangePasswordModel(UserManager<Register> userManager, SignInManager<Register> signInManager, AuthDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
			_context = context;

		}

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
			// Get the user
			var user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
			var passwordHistory = await _context.PasswordHistories.Where(x => x.UserId == user.Id).ToListAsync();
			if (user == null)
			{
				ModelState.AddModelError("", "User not found");
				return Page();
			}

			// Check whether the old password is correct
			var result = await _signInManager.UserManager.CheckPasswordAsync(user, CPModel.CurrentPassword);

			if (!result)
			{
				ModelState.AddModelError("", "Current password is incorrect");
				return Page();
			}

			// Check if the new password is the same as the old password
			var newResult = await _signInManager.UserManager.CheckPasswordAsync(user, CPModel.Password);

			if (newResult)
			{
				ModelState.AddModelError("", "New password cannot be the same as the old password");
				return Page();
			}

			// Check the password is in the most recent 2 passwords
			var historyResult = passwordHistory.Take(2).OrderByDescending(x => x.DateChanged);

			// If the password is the last 2 passwords, return an error
			foreach (var item in historyResult)
			{
				var result2 = _signInManager.UserManager.PasswordHasher.VerifyHashedPassword(user, item.Password, CPModel.Password);
				if (result2 == PasswordVerificationResult.Success)
				{
					ModelState.AddModelError("", "New password cannot be the same as the last 2 passwords");
					return Page();
				}
			}

			// Check whether a password change happened in the last 24 hours
			var lastChange = user.LastPasswordChange;
			var timeSpan = DateTime.Now - lastChange;
			if (timeSpan.TotalMinutes < 30)
			{
				ModelState.AddModelError("", "You can only change your password once every 30 minutes");
				return Page();
			}


			// Change the password
			await _signInManager.UserManager.ChangePasswordAsync(user, CPModel.CurrentPassword, CPModel.Password);

			// Add the password to the history
			user.PasswordHistory.Add(new PasswordHistory
			{
				UserId = user.Id,
				Password = _signInManager.UserManager.PasswordHasher.HashPassword(user, CPModel.Password),
				DateChanged = DateTime.Now
			});
			user.LastPasswordChange = DateTime.Now;
			return RedirectToPage("/Index"); // Redirect to home page or another page after successful password change.
        }
    }
}
