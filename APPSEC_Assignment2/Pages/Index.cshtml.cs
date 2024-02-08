using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using APPSEC_Assignment2.ViewModels;
using APPSEC_Assignment2.Model;

namespace APPSEC_Assignment2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
		private AuthDbContext _context { get; }

		private SignInManager<Register> signInManager { get; }
		private UserManager<Register> userManager { get; }

		public IndexModel(UserManager<Register> userManager, SignInManager<Register> signInManager, ILogger<IndexModel> logger, AuthDbContext context)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;

			_logger = logger;
			_context = context;

		}
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Gender { get; set; }
		public string NRIC { get; set; }
		public DateTime DOB { get; set; }
		public string WAI { get; set; }
		public string Resume { get; set; }
		
		public async Task<IActionResult> OnGet()
		{

			var user = await userManager.GetUserAsync(User);

			if (user != null)
			{
				// Check GUID
				var GUID = Request.Cookies["GUID"];

				if (user.GUID != GUID)
				{
					// GUID is not the same, audit and logout
					var audit = new Audit
					{
						Email = user.Email,
						Timestamp = DateTime.Now,
						Action = "Invalid GUID",
						Details = "Invalid GUID from another device"
					};

					_context.AuditLogs.Add(audit);
					await _context.SaveChangesAsync();
					await signInManager.SignOutAsync();
					return RedirectToPage("/Index");
				}


                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                Email = user.Email;
				FirstName = user.FirstName;
				LastName = user.LastName;
				Gender = user.Gender;
				NRIC = protector.Unprotect(user.NRIC);
				DOB = user.DOB;
				Resume = user.Resume;
				WAI = user.WAI;
			}

			return Page();
		}
	}
}
