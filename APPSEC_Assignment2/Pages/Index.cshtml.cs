using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace APPSEC_Assignment2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpContextAccessor contxt;

        public IndexModel(ILogger<IndexModel> logger, IHttpContextAccessor context)
        {
            _logger = logger;
            contxt = context;
        }

        public IActionResult OnGet()
        {




            var session = contxt.HttpContext.Session.GetString("AuthToken");

            if (session == null)
            {
                // Clear session
                contxt.HttpContext.Session.Clear();
                contxt.HttpContext.Session.Remove("Username");

                // Redirect to the Login page
                return RedirectToPage("/Login");
            }

            string encryptedEmail = contxt.HttpContext.Session.GetString("EncryptedEmail");
            string decryptedEmail = contxt.HttpContext.Session.GetString("DecryptedEmail");

            // Pass values to the Razor Page
            ViewData["EncryptedEmail"] = encryptedEmail;
            ViewData["DecryptedEmail"] = decryptedEmail;

            return Page();
        }
    }
}
