﻿using Bhbk.Lib.Identity.Data.Models;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace Bhbk.WebApi.Alert.Services
{
    public interface ITwilioService
    {
        ValueTask<MessageResource> TryTextHandoff(string sid, string token, uvw_TextQueue model);
    }
}
