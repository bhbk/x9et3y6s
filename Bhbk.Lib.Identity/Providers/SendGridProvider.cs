using Bhbk.Lib.Identity.Factory;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Providers
{
    public class SendgridProvider
    {
        public async Task<Response> TryEmailHandoff(string apiKey, UserCreateEmail model)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(model.FromEmail, model.FromDisplay);
            var to = new EmailAddress(model.ToEmail, model.ToDisplay);
            var subject = model.Subject;
            var plainTextContent = model.PlaintextContent;
            var htmlContent = model.HtmlContent;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            return await client.SendEmailAsync(msg);
        }
    }
}
