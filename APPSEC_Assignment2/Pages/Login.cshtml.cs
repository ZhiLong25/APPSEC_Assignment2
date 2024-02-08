using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using APPSEC_Assignment2.ViewModels;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using APPSEC_Assignment2.Model;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;
using SendGrid;
using SendGrid.Helpers.Mail;
using APPSEC_Assignment2.ViewModel;

namespace APPSEC_Assignment2.Pages
{
    public class LoginModel : PageModel
    {
		private readonly IHttpContextAccessor contxt;

        private readonly SignInManager<Register> signInManager;
        private readonly UserManager<Register> userManager;

        // private readonly AuthDbContext _context;
        private readonly EmailSender _emailSender;

        private readonly AuthDbContext _context;



        // Audit for Login
        private readonly AuditServiceModel _auditLogService;

        public LoginModel(IHttpContextAccessor httpContextAccessor, SignInManager<Register> signInManager, UserManager<Register> userManager, EmailSender emailSender, AuditServiceModel auditLogService, AuthDbContext context)
		{
            // _context = context;
            contxt = httpContextAccessor;

            this.signInManager = signInManager;
            _emailSender = emailSender;
            this.userManager = userManager;
            LModel = new Login();
            _auditLogService = auditLogService;
            _context = context;


        }

        [BindProperty]
        public Login LModel { get; set; }

        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            var recaptchaResponse = Request.Form["g-recaptcha-response"];

            // Validate the reCAPTCHA response using Google's reCAPTCHA API
            var recaptchaSecretKey = "6LfJx18pAAAAAOGkCG7M3JV5GSD9CEucOfRA8_UW";
            var client = new HttpClient();
            var response = await client.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={recaptchaSecretKey}&response={recaptchaResponse}");

            // Parse the response
            var recaptchaResult = JsonConvert.DeserializeObject<RecaptchaResponse>(response);

            // Check if the reCAPTCHA was successful
            if (recaptchaResult.Success)
            {
                if (ModelState.IsValid)
                {

                    var identityResult = await signInManager.PasswordSignInAsync(
                        LModel.Email,
                        LModel.Password,
                        LModel.RememberMe,
                        lockoutOnFailure: true // Enable lockout on failure
                    );

                    var user = await signInManager.UserManager.FindByEmailAsync(LModel.Email);
					// bool isTwoFactorEnabled = await signInManager.UserManager.GetTwoFactorEnabledAsync(user);

					if (identityResult.IsLockedOut)
                    {
                        TempData["AccountLocked"] = true;

                        ModelState.AddModelError("", "Account locked out, try again later");
                    }

                    else if (identityResult.RequiresTwoFactor)
                    {

                        var code = await signInManager.UserManager.GenerateTwoFactorTokenAsync(user, "Email");
                        await _emailSender.SendEmailAsync(LModel.Email, "Welcome to YourApp!", code);

                        var audit = new Audit
                        {
                            Email = user.Email,
                            Timestamp = DateTime.Now,
                            Action = "Login",
                            Details = $"Asking to verify 2FA for {LModel.Email}"
                        };

                        _context.AuditLogs.Add(audit);
                        await _context.SaveChangesAsync();

                        return RedirectToPage("/2fa", new { Email = LModel.Email });
                    }

                    else if (identityResult.Succeeded)
                    {

						var GUID = Request.Cookies["GUID"];
						if (user.GUID != null)
						{
                            if (user.GUID != GUID)
                            {
                                // GUID is not the same, audit and logout

                                var audit2 = new Audit
                                {
                                    Email = user.Email,
                                    Timestamp = DateTime.Now,
                                    Action = "Login",
                                    Details = $"Multiple Device detected {LModel.Email}"
                                };

                                _context.AuditLogs.Add(audit2);
                                await _context.SaveChangesAsync();


								await signInManager.SignOutAsync();
								return RedirectToPage("/Index");
							}
						}

						// Get the user
                        await signInManager.UserManager.UpdateAsync(user);

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

                        var audit = new Audit
                        {
                            Email = user.Email,
                            Timestamp = DateTime.Now,
                            Action = "Login",
                            Details = "Login attempt"
                        };

                        _context.AuditLogs.Add(audit);
                        await _context.SaveChangesAsync();

                        return RedirectToPage("Index");
                    }
                    else
                    {
                        var audit = new Audit
                        {
                            Email = user.Email,
                            Timestamp = DateTime.Now,
                            Action = "Login",
                            Details = $"Failed login attempt from {LModel.Email}"
                        };

                        _context.AuditLogs.Add(audit);
                        await _context.SaveChangesAsync();

						ModelState.AddModelError("", "Username or Password incorrect");
					}

                }
                
            }
            else
            {
                ModelState.AddModelError("", $"reCAPTCHA validation failed. Please try again.{recaptchaResult.Success} {response}");
            }
            return Page();
        }

        
        public IActionResult Index()
		{

            return Page();

        }


        public void OnGet()
        {
    
        }

    }
}
