using Bhbk.Lib.Identity.DomainModels.Alert;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Bhbk.WebApi.Alert.Providers
{
    public class TwilioProvider
    {
        public async Task TryTextHandoff(string sid, string token, TextCreate model)
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
