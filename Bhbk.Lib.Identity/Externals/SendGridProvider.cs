using Bhbk.Lib.Identity.Factory;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Externals
{
    public class SendgridProvider
    {
        public async Task TryEmailHandoff(string apiKey, UserCreateEmail email)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(email.FromEmail, email.FromDisplay);
            var to = new EmailAddress(email.ToEmail, email.ToDisplay);
            var subject = email.Subject;
            var plainTextContent = email.PLaintextContent;
            var htmlContent = email.HtmlContent;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var result = await client.SendEmailAsync(msg);
        }
    }
}
