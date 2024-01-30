namespace APPSEC_Assignment2.ViewModels
{
    public class RecaptchaResponse
    {
        public bool Success { get; set; }
        public string ChallengeTs { get; set; }
        public string Hostname { get; set; }
        // Add other properties as needed
    }
}
