using APPSEC_Assignment2.Model;
using APPSEC_Assignment2.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace APPSEC_Assignment2.Pages
{
    public class Activate2FAModel : PageModel
    {
        private readonly AuthDbContext _context;

        private readonly SignInManager<Register> signInManager;
        public Activate2FAModel(SignInManager<Register> signInManager, AuthDbContext context)
        {
            this.signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await signInManager.UserManager.GetUserAsync(User);

            user.TwoFactorEnabled = true;
            user.EmailConfirmed = true;
            await signInManager.UserManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }
    }
}
