namespace APPSEC_Assignment2.ViewModel
{
	public class PasswordHistory
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Password { get; set; }
		public DateTime DateChanged { get; set; }
	}
}
