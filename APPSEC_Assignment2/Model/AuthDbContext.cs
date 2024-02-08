using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using APPSEC_Assignment2.ViewModels;
using System.Collections.Generic;
using APPSEC_Assignment2.ViewModel;

namespace APPSEC_Assignment2.Model
{
    public class AuthDbContext : IdentityDbContext
    {
        public DbSet<Audit> AuditLogs { get; set; }

        public DbSet<Register> RegisteredUsers { get; set; }

		public DbSet<PasswordHistory> PasswordHistory { get; set; }

		private readonly IConfiguration _configuration;
        //public AuthDbContext(DbContextOptions<AuthDbContext> options):base(options){ }
        public AuthDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = _configuration.GetConnectionString("AuthConnectionString"); optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
