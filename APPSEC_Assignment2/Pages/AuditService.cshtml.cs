using APPSEC_Assignment2.Model;
using APPSEC_Assignment2.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APPSEC_Assignment2.Pages
{
    public class AuditServiceModel : PageModel
    {
        private readonly AuthDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditServiceModel(AuthDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

    }
}
