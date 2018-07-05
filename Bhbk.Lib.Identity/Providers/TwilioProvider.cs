using Bhbk.Lib.Identity.Factory;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Bhbk.Lib.Identity.Providers
{
    public class TwilioProvider
    {
        public async Task TryTextHandoff(string sid, string token, UserCreateText model)
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
