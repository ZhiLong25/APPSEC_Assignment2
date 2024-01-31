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
            mailBody.AppendFormat("<h1>User Registered</h1>");
            mailBody.AppendFormat("<br />");
            mailBody.AppendFormat($"<p>{body}</p>");
            mailMessage.Body = mailBody.ToString();

            // Send email
            client.Send(mailMessage);


            //var smtpClient = new SmtpClient("smtp.gmail.com")
            //{
            //    Port = 587,
            //    Credentials = new NetworkCredential("shotmanpuru@gmail.com", "puru69shotman"),
            //    EnableSsl = true,
            //};

            //var mailMessage = new MailMessage
            //{
            //    From = new MailAddress("shotmanpuru@gmail.com"),
            //    Subject = subject,
            //    Body = body,
            //    IsBodyHtml = false, // Set to true if the body contains HTML
            //};

            //mailMessage.To.Add(to);

            //await smtpClient.SendMailAsync(mailMessage);
        }

    }
}
