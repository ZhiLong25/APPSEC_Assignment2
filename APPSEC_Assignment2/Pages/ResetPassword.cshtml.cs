using APPSEC_Assignment2.Migrations;
using APPSEC_Assignment2.Model;
using APPSEC_Assignment2.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;

namespace APPSEC_Assignment2.Pages
{
    public class ResetPasswordModel : PageModel
    {

        [BindProperty]
		[Required]
		public string NewPassword { get; set; }


		[BindProperty]
		[DataType(DataType.Password)]
		[Required]
		[Compare(nameof(NewPassword), ErrorMessage = "Password and confirmation password does not match")]
		public string ConfirmPassword { get; set; }


		private readonly AuthDbContext _context;

        private readonly SignInManager<Register> _signInManager;
		private readonly UserManager<Register> _userManager;

		public ResetPasswordModel(SignInManager<Register> signInManager, UserManager<Register> userManager, AuthDbContext context)
        {
            _userManager = userManager;
			_signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> OnPostAsync(string token, string UserId)
        {
            if (ModelState.IsValid)
            {

				var user = await _signInManager.UserManager.FindByIdAsync(UserId);

				if (user == null)
                {
                    return RedirectToPage("/Index");
                }
				else
				{
                    var passwordHistory = await _context.PasswordHistory.Where(x => x.UserId == user.Id).ToListAsync();

                    // Check if the new password is the same as the old password
                    var samePass = await _signInManager.UserManager.CheckPasswordAsync(user, NewPassword);

                    if (samePass)
                    {
                        ModelState.AddModelError("", "New password cannot be the same as the old password");
                        return Page();
                    }
                    else
                    {
                        // Take 2 previous passwords
                        var history = passwordHistory.OrderByDescending(x => x.DateChanged).Take(2);
                        foreach (var item in history)
                        {
                            var checkUsedPass = _signInManager.UserManager.PasswordHasher.VerifyHashedPassword(user, item.Password, NewPassword);
                            if (checkUsedPass == PasswordVerificationResult.Success)
                            {
                                ModelState.AddModelError("", "Password cannot be the same as the last 2 passwords");
                                return Page();
                            }
                        }

                        var reset = await _signInManager.UserManager.ResetPasswordAsync(user, token, NewPassword);

                        if (reset.Succeeded)
                        {
                            // Add the new password to the password history
                            var passHistory = new ViewModel.PasswordHistory
                            {
                                UserId = user.Id,
                                Password = NewPassword,
                                DateChanged = DateTime.Now
                            };


                            var audit = new Audit
                            {
                                Email = user.Email,
                                Timestamp = DateTime.Now,
                                Action = "Password reset",
                                Details = "Password has been reset"
                            };

                            user.LastPasswordChange = DateTime.Now;
                            _context.PasswordHistory.Add(passHistory);
                            _context.AuditLogs.Add(audit);

                            await _context.SaveChangesAsync();
                            await _signInManager.UserManager.UpdateAsync(user);

                            await _signInManager.SignOutAsync();

                            return RedirectToPage("/Index");
                        }
                        else
                        {
                            return RedirectToPage("/Index");
                        }
                    }
                }

                
            }
            return Page();
        }

    }
}
