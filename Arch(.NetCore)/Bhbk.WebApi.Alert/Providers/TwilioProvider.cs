using Bhbk.Lib.Identity.Data.Models;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Bhbk.WebApi.Alert.Helpers
{
    public class TwilioProvider
    {
        public async ValueTask TryTextHandoff(string sid, string token, tbl_QueueTexts model)
        {
            TwilioClient.Init(sid, token);

            var text = await MessageResource.CreateAsync(
                body: model.Body,
                from: new Twilio.Types.PhoneNumber(model.FromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(model.ToPhoneNumber)
            );
        }
    }
}
