using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using APPSEC_Assignment2.ViewModels;

namespace APPSEC_Assignment2.Pages
{
    public class LogoutModel : PageModel
    {
		private readonly SignInManager<Register> signInManager;
		private UserManager<Register> userManager { get; }

		public LogoutModel(SignInManager<Register> signInManager, UserManager<Register> userManager)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
		}
		public void OnGet() { }
		public async Task<IActionResult> OnPostLogoutAsync()
        {
			var user = await userManager.GetUserAsync(User);
			user.GUID = null;
			await signInManager.UserManager.UpdateAsync(user);

			await signInManager.SignOutAsync();

            HttpContext.Session.Clear();

			
            return RedirectToPage("Login");
		}
		public async Task<IActionResult> OnPostDontLogoutAsync()
		{
			return RedirectToPage("Index");
		}



	}
}
