using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using APPSEC_Assignment2.ViewModels;  // Add the namespace for your ViewModel
using APPSEC_Assignment2.ViewModel;

namespace APPSEC_Assignment2.Pages
{
    public class _2FAModel : PageModel
    {   
        private readonly UserManager<Register> userManager;
        private readonly SignInManager<Register> signInManager;

        public TwoFactorAuthViewModel TwoFactorAuthViewModel { get; set; }

        private string email;
        private string password;
        private bool? rememberMe;
        public _2FAModel(UserManager<Register> userManager, SignInManager<Register> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            TwoFactorAuthViewModel = new TwoFactorAuthViewModel();
        }

        [BindProperty]
        public string verificationField { get; set; }

        public void OnGet()
        {
             //email = TempData["Email"];
             //password = TempData["Password"];
             //rememberMe = TempData["RememberMe"];

        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await userManager.FindByIdAsync(TwoFactorAuthViewModel.UserId);

            var verificationCode = "test";
            if (verificationField == verificationCode)
            {
                //var identityResult = await signInManager.PasswordSignInAsync(
                //   email,
                //   password,
                //    rememberMe.GetValueOrDefault(), // Use GetValueOrDefault to convert bool? to bool
                //    lockoutOnFailure: true // Enable lockout on failure
                //);

                //if (identityResult.IsLockedOut)
                //{
                //    return RedirectToPage("/ChangePassword");
                //}

                return RedirectToPage("/Index");

            }
            else
            {
                ModelState.AddModelError("", "Invalid Two-Factor Authentication code");
                return Page();
            }

            //// Verify the Two-Factor Authentication code
            //var result = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, TwoFactorAuthViewModel.Code);

            //if (result)
            //{
            //    // Two-Factor Authentication successful, sign in the user
            //    await signInManager.SignInAsync(user, isPersistent: false);
            //    return RedirectToPage("/Index");
            //}
            //else
            //{
            //    ModelState.AddModelError("", "Invalid Two-Factor Authentication code");
            //    return Page();
            //}
        }
    }
}
