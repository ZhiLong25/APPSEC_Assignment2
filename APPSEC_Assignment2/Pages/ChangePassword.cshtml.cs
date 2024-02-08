using APPSEC_Assignment2.Model;
using APPSEC_Assignment2.ViewModel;
using APPSEC_Assignment2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace APPSEC_Assignment2.Pages
{
	[Authorize(Policy = "LoggedIn")]
	public class ChangePasswordModel : PageModel
    {

		[BindProperty]
		[Required]
		[DataType(DataType.Password)]
		public string CurrentPassword { get; set; }

		[BindProperty]
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[BindProperty]
		[Required]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "Passwords does not match")]
		public string ConfirmPassword { get; set; }

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


			var lastChange = user.LastPasswordChange;
			var duration = DateTime.Now - lastChange;
			if (duration.TotalMinutes > 2)
			{
				ModelState.AddModelError("", "You need to reset your password now password age (2 minute)");

			}

			return Page();

        }


        public async Task<IActionResult> OnPostAsync()
        {
			// Get the user
			var user = await _userManager.GetUserAsync(User);
			var passwordHistory = await _context.PasswordHistory.Where(x => x.UserId == user.Id).ToListAsync();
			

			var checkPass = await _signInManager.UserManager.CheckPasswordAsync(user, Password);

			if (checkPass)
			{
				ModelState.AddModelError("", "New password cannot be the same as the old password");
				return Page();
			}

            // Check if it exist in 2 previous passwords
            var historyResult = passwordHistory.OrderByDescending(x => x.DateChanged).Take(2);
            foreach (var item in historyResult)
			{
				var checkExist = _signInManager.UserManager.PasswordHasher.VerifyHashedPassword(user, item.Password, Password);
				if (checkExist == PasswordVerificationResult.Success)
				{
					ModelState.AddModelError("", "New password cannot be the same as the last 2 passwords");
					return Page();
				}
			}

			// Check password last changed timeline
			var lastChange = user.LastPasswordChange;
			var duration = DateTime.Now - lastChange;
			if (duration.TotalMinutes < 1)
			{
				ModelState.AddModelError("", "You can only change your password once every 1 minute");
				return Page();
			}

			await _signInManager.UserManager.ChangePasswordAsync(user, CurrentPassword, Password);
			user.PasswordHistory.Add(new PasswordHistory
			{
				UserId = user.Id,
				Password = _signInManager.UserManager.PasswordHasher.HashPassword(user, Password),
				DateChanged = DateTime.Now
			});
			user.LastPasswordChange = DateTime.Now;

			await _context.SaveChangesAsync();

			return RedirectToPage("/Index"); 
        }
    }
}
