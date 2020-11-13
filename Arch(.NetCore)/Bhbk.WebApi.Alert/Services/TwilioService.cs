using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Bhbk.WebApi.Alert.Services
{
    /*
     * https://www.twilio.com/docs
     */
    public class TwilioService : ITwilioService
    {
        public async ValueTask<MessageResource> TryTextHandoff(string sid, string token, tbl_TextQueue model)
        {
            TwilioClient.Init(sid, token);

            return await MessageResource.CreateAsync(
                body: model.Body,
                from: new Twilio.Types.PhoneNumber(model.FromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(model.ToPhoneNumber)
            );
        }
    }
}
