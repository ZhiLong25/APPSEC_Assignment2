using System.Net.Mail;
using System.Net;
using System.Text;

namespace APPSEC_Assignment2.Model
{
    public class EmailSender
    {
        // Define a method to send an email
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Set up SMTP client
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("tanzhilong25@gmail.com", "ngnd cqdj rhqi zlvx\r\n");

            // Create email message
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("tanzhilong25@gmail.com");
            mailMessage.To.Add(to);
            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = true;
            StringBuilder mailBody = new StringBuilder();
            mailBody.AppendFormat("<h1>APPSEC Application</h1>");
            mailBody.AppendFormat("<br />");
            mailBody.AppendFormat($"<p>{body}</p>");
            mailMessage.Body = mailBody.ToString();

            // Send email
            client.Send(mailMessage);

        }

    }
}
