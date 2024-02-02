using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using APPSEC_Assignment2.ViewModels;  // Add the namespace for your ViewModel
using APPSEC_Assignment2.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using APPSEC_Assignment2.Model;

namespace APPSEC_Assignment2.Pages
{
    [Authorize(policy: "LoggedIn")]
    public class _2FAModel : PageModel
    {   
        private readonly UserManager<Register> userManager;
        private readonly SignInManager<Register> signInManager;
        private readonly AuthDbContext _context;


        private string email;
        private string password;
        private bool? rememberMe;
        public _2FAModel(UserManager<Register> userManager, SignInManager<Register> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;

        }

        [BindProperty]
        public string verificationField { get; set; }

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
        public async Task<IActionResult> OnPostAsync(string Email)
        {
            var result = await signInManager.TwoFactorSignInAsync("Email", verificationField, false, false);
            if (result.Succeeded)
            {
                // Get the user
                var user = await signInManager.UserManager.FindByEmailAsync(Email);

                // Create GUID
                var guid = Guid.NewGuid().ToString();
                user.GUID = guid;
                await signInManager.UserManager.UpdateAsync(user);

                //Create the security context
                var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("Department", "HR"),
                    };

                Response.Cookies.Append("GUID", guid, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                });

                var i = new ClaimsIdentity(claims, "MyCookieAuth");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(i);
                await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

                // Create audit record
                var audit = new Audit
                {
                    Email = user.Email,
                    Timestamp = DateTime.Now,
                    Action = "Login",
                };

                _context.AuditLogs.Add(audit);
                await _context.SaveChangesAsync();
                return RedirectToPage("/Index");

            }
            else
            {
                ModelState.AddModelError("", "Invalid code");
                return Page();
            }

        }
    }
}
