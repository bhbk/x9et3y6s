using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Lookups.V1;

namespace Bhbk.WebApi.Alert.Helpers
{
    /*
     * https://www.twilio.com/docs
     */
    public class TwilioProvider
    {
        public async ValueTask<MessageResource> TryTextHandoff(string sid, string token, tbl_QueueText model)
        {
            TwilioClient.Init(sid, token);

            /*
             * https://www.twilio.com/docs/lookup/quickstart
             */
            var lookup = PhoneNumberResource.Fetch(
                pathPhoneNumber: new Twilio.Types.PhoneNumber(model.ToPhoneNumber));

            return await MessageResource.CreateAsync(
                body: model.Body,
                from: new Twilio.Types.PhoneNumber(model.FromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(model.ToPhoneNumber)
            );
        }
    }
}
