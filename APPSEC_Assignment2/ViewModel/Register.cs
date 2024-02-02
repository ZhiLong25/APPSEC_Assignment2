using APPSEC_Assignment2.ViewModel;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APPSEC_Assignment2.ViewModels
{
    public class Register : IdentityUser
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "NRIC is required")]
        public string NRIC { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{12,}$",
            ErrorMessage = "Password must be at least 12 characters long and include a combination of lowercase, uppercase, numbers, and special characters.")]
        public string Password { get; set; }

        [NotMapped]

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        public string? Resume { get; set; }

        [Required(ErrorMessage = "Who am I is required")]
        public string WAI { get; set; }

        public string GUID { get; set; } = string.Empty;

		public DateTime LastPasswordChange { get; set; } = DateTime.Now;

		public List<PasswordHistory> PasswordHistory { get; set; } = new List<PasswordHistory>();

		public DateTime PasswordChangedDate { get; set; } = DateTime.Now;

        public string? EncryptedEmail { get; set; }
        public string? DecryptedEmail { get; set; }
    }
}
