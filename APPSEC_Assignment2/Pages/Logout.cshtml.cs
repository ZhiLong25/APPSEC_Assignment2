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
		public LogoutModel(SignInManager<Register> signInManager)
		{
			this.signInManager = signInManager;
		}
		public void OnGet() { }
		public async Task<IActionResult> OnPostLogoutAsync()
        {
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
