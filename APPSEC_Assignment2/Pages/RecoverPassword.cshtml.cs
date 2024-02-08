using APPSEC_Assignment2.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Net.Mail;
using System.Net;
using APPSEC_Assignment2.ViewModels;
using Castle.Core.Smtp;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace APPSEC_Assignment2.Pages
{
    public class RecoverPasswordModel : PageModel
    {
		[BindProperty]
		public string Email { get; set; }

		private readonly AuthDbContext _context;
		private readonly EmailSender _emailSender;

		private readonly SignInManager<Register> signInManager;
		private readonly UserManager<Register> userManager;
		public RecoverPasswordModel(SignInManager<Register> signInManager,AuthDbContext context,UserManager<Register> userManager, EmailSender emailSender)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
			_context = context;
			_emailSender = emailSender;
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{
				var user = userManager.FindByEmailAsync(Email).Result;
				if (user == null)
				{
					return RedirectToPage("/Login");
				}

				var token = await userManager.GeneratePasswordResetTokenAsync(user);

				var link = Url.Page("/ResetPassword", null, new { token, UserId = user.Id }, Request.Scheme, Request.Host.ToString());

				await _emailSender.SendEmailAsync(Email, "Reset Password", $"Click here to reset your password: {link}");

				return RedirectToPage("/Login");
			}
			return Page();
		}
	}
}
