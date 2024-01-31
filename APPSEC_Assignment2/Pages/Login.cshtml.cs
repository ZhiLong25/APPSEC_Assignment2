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


        // Audit for Login
        private readonly AuditServiceModel _auditLogService;

        public LoginModel(IHttpContextAccessor httpContextAccessor, SignInManager<Register> signInManager, UserManager<Register> userManager, EmailSender emailSender, AuditServiceModel auditLogService)
		{
            // _context = context;
            contxt = httpContextAccessor;

            this.signInManager = signInManager;
                _emailSender = emailSender;
            this.userManager = userManager;
            LModel = new Login();
            _auditLogService = auditLogService;

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


                    var user = await userManager.FindByNameAsync(LModel.Email);

                    var identityResult = await signInManager.PasswordSignInAsync(
                        LModel.Email,
                        LModel.Password,
                        LModel.RememberMe,
                        lockoutOnFailure: true // Enable lockout on failure
                    );

                    if (identityResult.IsLockedOut)
                    {
                        return RedirectToPage("/ChangePassword");
                    }


                    if (user != null && BCrypt.Net.BCrypt.Verify(LModel.Password, user.Password))
                    {
                        contxt.HttpContext.Session.SetString("Username", LModel.Email);




                        // User authentication successful, check if 2FA is enabled
                        //if (await userManager.GetTwoFactorEnabledAsync(user))
                        //{
                        //    // Generate a Two-Factor Authentication code
                        //    var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");

                        //    // Send the token to the user (e.g., via email or SMS)
                        //    await SendEmailAsync(user.UserName, "Your 2FA Code", $"Your 2FA code is: {token}");

                        //    // Redirect to a page where the user enters the code
                        //    return RedirectToAction("2FA", new { userId = user.Id });
                        //}



                        string guid = Guid.NewGuid().ToString();
                        contxt.HttpContext.Session.SetString("AuthToken", guid);

                        contxt.HttpContext.Response.Cookies.Append("AuthToken", guid, new CookieOptions
                        {
                            Expires = DateTime.Now.AddSeconds(10),
                            HttpOnly = true,
                            SameSite = SameSiteMode.Strict,
                            Secure = true
                        });


                        var claims = new List<Claim> {
                            new Claim(ClaimTypes.Email, user.UserName),
                        };

                        var i = new ClaimsIdentity(claims, "MyCookieAuth");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(i);
                        await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

                        //var timeSinceLastChange = DateTime.UtcNow - user.PasswordChangedDate;

                        //if (timeSinceLastChange > TimeSpan.FromMinutes(1))
                        //{
                        //    TempData["AlertMessage"] = "Password has to be changed every 1 minute, login to change";
                        //    return RedirectToPage("/ChangePassword");
                        //}

                        var verificationCode = "test";

                        _auditLogService.LogUserActivity(LModel.Email, "Login", $"Log in successful {LModel.Email}");



                        await _emailSender.SendEmailAsync(LModel.Email, "Welcome to YourApp!", verificationCode);


                        return RedirectToPage("/2fa");

                    }
                    else
                    {
                        _auditLogService.LogUserActivity(LModel.Email, "Login", $"Failed login attempt from {LModel.Email}");
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
