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
    public class _2FAModel : PageModel
    {   
        private readonly UserManager<Register> userManager;
        private readonly SignInManager<Register> signInManager;
        private readonly AuthDbContext _context;


        public _2FAModel(UserManager<Register> userManager, SignInManager<Register> signInManager, AuthDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public string verificationField { get; set; }

        public async Task<IActionResult> OnPostAsync(string Email)
        {
            var result = await signInManager.TwoFactorSignInAsync("Email", verificationField, false, false);

            // 2FA Passed
            if (result.Succeeded)
            {
                var user = await signInManager.UserManager.FindByEmailAsync(Email);

                // Set new GUID
                var guid = Guid.NewGuid().ToString();
                user.GUID = guid;
                await signInManager.UserManager.UpdateAsync(user);

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

                var audit = new Audit
                {
                    Email = user.Email,
                    Timestamp = DateTime.Now,
                    Action = "Login",
                    Details = "2FA Successful"
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
