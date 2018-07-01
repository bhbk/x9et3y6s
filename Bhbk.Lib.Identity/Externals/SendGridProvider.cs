using Bhbk.Lib.Identity.Factory;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace Bhbk.Lib.Identity.Externals
{
    public class SendgridProvider
    {
        public async Task<Response> TryEmailHandoff(string apiKey, UserCreateEmail model)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(model.FromEmail, model.FromDisplay);
            var to = new EmailAddress(model.ToEmail, model.ToDisplay);
            var subject = model.Subject;
            var plainTextContent = model.PLaintextContent;
            var htmlContent = model.HtmlContent;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            return await client.SendEmailAsync(msg);
        }
    }
}
