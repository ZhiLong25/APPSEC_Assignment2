namespace APPSEC_Assignment2.ViewModels
{
    public class Audit
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
