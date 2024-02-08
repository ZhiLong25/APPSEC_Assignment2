using APPSEC_Assignment2.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Web;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Encodings.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace APPSEC_Assignment2.Pages
{
    //Initialize the build-in ASP.NET Identity
    public class RegisterModel : PageModel
    {

        [BindProperty]
        public Register RModel { get; set; }

        [BindProperty]
        public IFormFile Resume { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "Password is required")]
		[DataType(DataType.Password)]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{12,}$",
	    ErrorMessage = "Password must be at least 12 characters long and include a combination of lowercase, uppercase, numbers, and special characters.")]
		public string Password { get; set; }

        [BindProperty]
		[Required(ErrorMessage = "Confirm Password is required")]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match")]
		public string ConfirmPassword { get; set; }

		// private readonly AuthDbContext _context;
		private readonly IWebHostEnvironment _environment;


        private readonly UserManager<Register> _userManager;
        private readonly SignInManager<Register> _signInManager;

        public RegisterModel(IWebHostEnvironment environment, UserManager<Register> userManager, SignInManager<Register> signInManager)
        {
            _environment = environment;
            _userManager = userManager;
            _signInManager = signInManager;
            //_context = context;

        }

        private static RijndaelManaged cipher = new RijndaelManaged();
        ICryptoTransform encryptTransform = cipher.CreateEncryptor();
        ICryptoTransform decryptTransform = cipher.CreateDecryptor();


        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            var file_path_overall = "";

            if (ModelState.IsValid)
            {

                long FILE_SIZE = 2 * 1024 * 1024;

                if (Resume.Length > FILE_SIZE)
                {
                    ModelState.AddModelError(nameof(Resume), "File size exceeds the allowed limit.");
                    return Page();
                }

                string[] allowedExtensions = { ".pdf", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(Resume.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError(nameof(Resume), "Only .pdf, .doc, .docx allowed");
                    return Page();
                }
                else
                {
                    file_path_overall = randomFileString(fileExtension);
                    using (var File_Stream = new FileStream(file_path_overall, FileMode.Create))
                    {
                        await Resume.CopyToAsync(File_Stream);
                    }
                }

                var existingUser = await _userManager.FindByEmailAsync(RModel.Email);

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
                
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                var user = new Register
                {
                    UserName = HttpUtility.HtmlEncode(RModel.Email),
                    Email = RModel.Email,
                    FirstName = HttpUtility.HtmlEncode(RModel.FirstName),
                    LastName = HttpUtility.HtmlEncode(RModel.LastName),
                    Gender = HttpUtility.HtmlEncode(RModel.Gender),
                    NRIC = protector.Protect(RModel.NRIC),
                    Resume = file_path_overall,
                    DOB = RModel.DOB,
                    WAI = HttpUtility.HtmlEncode(RModel.WAI),
                    GUID = null,
                    EncryptedEmail = encEmail,
                    DecryptedEmail = decEmail

                };

                var result = await _userManager.CreateAsync(user, Password);

                // Hash the password using BCrypt
                //string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);
                //user.PasswordHash = hashedPassword;

                if (result.Succeeded)
                {
                    // await _userManager.SetTwoFactorEnabledAsync(user, true);
                    return RedirectToPage("Index");
                }
                else
                {
                    ModelState.AddModelError("", $"Failed to store the user in the database. {RModel.Email} {result}");
                    // Log or print errors
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

            }
            else
            {
                ModelState.AddModelError("", $"Failed to {RModel.Resume}.");
                //return new RedirectToPageResult("/Error", new { statusCode = 500 });
            }

            return Page();
        }


        private string randomFileString(string file)
        {
            var random = new Random();
            var rid = random.Next(1, 100000000);
            var filePath = Path.Combine(".\\resume", rid + file);

            if (System.IO.File.Exists(filePath))
            {
                return randomFileString(file);
            }
            else
            {
                return filePath;
            }
        }

    }


}