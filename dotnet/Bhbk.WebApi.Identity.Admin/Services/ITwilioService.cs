using Bhbk.Lib.Identity.Data.EF.Models;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace Bhbk.WebApi.Identity.Admin.Services
{
    public interface ITwilioService
    {
        ValueTask<MessageResource> TryTextHandoff(string sid, string token, tbl_TextQueue model);
    }
}
