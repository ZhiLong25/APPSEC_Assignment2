using System.ComponentModel.DataAnnotations;

namespace APPSEC_Assignment2.ViewModel
{
	public class ChangePassword
	{
		[Required]
		[DataType(DataType.Password)]
		public string CurrentPassword { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		[Required]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
		public string ConfirmPassword { get; set; }
	}
}
