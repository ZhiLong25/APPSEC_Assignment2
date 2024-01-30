using APPSEC_Assignment2.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using BCrypt.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using APPSEC_Assignment2.Model;
using System.Linq;
using System.Web;

namespace APPSEC_Assignment2.Pages
{
    //Initialize the build-in ASP.NET Identity
    public class RegisterModel : PageModel
    {

        [BindProperty]
        public Register RModel { get; set; }

        // private readonly AuthDbContext _context;

        private readonly UserManager<Register> _userManager;
        private readonly SignInManager<Register> _signInManager;

        public RegisterModel(UserManager<Register> userManager, SignInManager<Register> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_context = context;

        }

        private static RijndaelManaged cipher = new RijndaelManaged();
        ICryptoTransform encryptTransform = cipher.CreateEncryptor();
        ICryptoTransform decryptTransform = cipher.CreateDecryptor();


        [ValidateAntiForgeryToken]
        //Save data into the database
        public async Task<IActionResult> OnPostAsync()
        {

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(RModel.Email);

                //var existingUser = _context.RegisteredUsers.FirstOrDefault(u => u.Email == RModel.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Email address is already in use.");
                    return Page();
                }







                // Encryption
                byte[] emailTxt = Encoding.UTF8.GetBytes(RModel.Email);
                byte[] cipherEmail = encryptTransform.TransformFinalBlock(emailTxt, 0, emailTxt.Length);
                RModel.EncryptedEmail = Convert.ToBase64String(cipherEmail);
                var encEmail = Convert.ToBase64String(cipherEmail);

                // Decryption
                byte[] DecEmail = decryptTransform.TransformFinalBlock(cipherEmail, 0, cipherEmail.Length);
                RModel.DecryptedEmail = Encoding.UTF8.GetString(DecEmail);
                var decEmail = Encoding.UTF8.GetString(DecEmail);

                var user = new Register
                {
                    UserName = HttpUtility.HtmlEncode(RModel.Email),
                    Email = HttpUtility.HtmlEncode(RModel.Email),
                    FirstName = HttpUtility.HtmlEncode(RModel.FirstName),
                    Password = HttpUtility.HtmlEncode(RModel.Password),
                    LastName = HttpUtility.HtmlEncode(RModel.LastName),
                    Gender = HttpUtility.HtmlEncode(RModel.Gender),
                    NRIC = HttpUtility.HtmlEncode(RModel.NRIC),
                    Resume = HttpUtility.HtmlEncode(RModel.Resume),
                    DOB = RModel.DOB,
                    WAI = HttpUtility.HtmlEncode(RModel.WAI),

                    EncryptedEmail = encEmail,
                    DecryptedEmail = decEmail

                };

                // Hash the password using BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(RModel.Password);
                user.Password = hashedPassword;

                //_context.RegisteredUsers.Add(user);

                // var saveResult = await _context.SaveChangesAsync();

                try
                {
                    var result = await _userManager.CreateAsync(user, RModel.Password);

                    if (result.Succeeded)
                    {
                        await _userManager.SetTwoFactorEnabledAsync(user, true);

                        HttpContext.Session.SetString("EncryptedEmail", RModel.EncryptedEmail);
                        HttpContext.Session.SetString("DecryptedEmail", RModel.DecryptedEmail);

                        return RedirectToPage("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to store the user in the database.");
                        // Log or print errors
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log or print the exception
                    ModelState.AddModelError("", "An error occurred while processing your request.");
                }

            }
            else
            {
                ModelState.AddModelError("", $"Failed to {RModel.Resume}.");
                //return new RedirectToPageResult("/Error", new { statusCode = 500 });
            }

            return Page();
        }

    }
}

