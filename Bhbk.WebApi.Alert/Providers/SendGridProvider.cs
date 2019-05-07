using Bhbk.Lib.Identity.Data.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Providers
{
    public class SendgridProvider
    {
        public async Task<Response> TryEmailHandoff(string apiKey, tbl_QueueEmails model)
        {
            var client = new SendGridClient(apiKey);

            var msg = MailHelper.CreateSingleEmail(
                new EmailAddress(model.FromEmail, model.FromDisplay),
                new EmailAddress(model.ToEmail, model.ToDisplay),
                model.Subject,
                model.PlaintextContent,
                model.HtmlContent);

            msg.SetReplyTo(new EmailAddress(model.FromEmail, model.FromDisplay));
            msg.SetSendAt(Int32.Parse(((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString()));
            msg.SetSpamCheck(false);
            msg.SetClickTracking(false, false);
            msg.SetFooterSetting(false);

            return await client.SendEmailAsync(msg);
        }
    }
}
