using APPSEC_Assignment2.Model;
using APPSEC_Assignment2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata;

namespace APPSEC_Assignment2.Pages
{
    [Authorize(policy: "LoggedIn")]
    public class Enable2FAModel : PageModel
    {

        private readonly AuthDbContext _context;

        private readonly SignInManager<Register> signInManager;
        public Enable2FAModel(SignInManager<Register> signInManager, AuthDbContext context)
        {
            this.signInManager = signInManager;
            _context = context;
        }

        public void OnPost()
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await signInManager.UserManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index");
            }

            user.TwoFactorEnabled = true;
            user.EmailConfirmed = true;
            await signInManager.UserManager.UpdateAsync(user);
            return RedirectToPage("/Index");
        }
    }
}
