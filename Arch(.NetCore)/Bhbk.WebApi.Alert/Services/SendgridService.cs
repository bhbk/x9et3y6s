using Bhbk.Lib.Identity.Data.EFCore.Models_TSQL;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Services
{
    /*
     * https://sendgrid.com/docs
     */
    public class SendgridService : ISendgridService
    {
        public async ValueTask<Response> TryEmailHandoff(string apiKey, uvw_EmailQueue model)
        {
            var provider = new SendGridClient(apiKey);
            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress(model.FromEmail, model.FromDisplay));
            msg.Subject = model.Subject;
            msg.AddTo(new EmailAddress(model.ToEmail, model.ToDisplay));
            msg.AddContent("text/html", model.Body);

            msg.SetReplyTo(new EmailAddress(model.FromEmail, model.FromDisplay));
            msg.SetSendAt(Int32.Parse(((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString()));
            msg.SetSpamCheck(false);
            msg.SetClickTracking(false, false);
            msg.SetFooterSetting(false);

            return await provider.SendEmailAsync(msg);
        }
    }
}
