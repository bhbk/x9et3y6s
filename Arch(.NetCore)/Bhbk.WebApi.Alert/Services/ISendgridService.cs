﻿using Bhbk.Lib.Identity.Data.EFCore.Models_TSQL;
using SendGrid;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Services
{
    public interface ISendgridService
    {
        ValueTask<Response> TryEmailHandoff(string apiKey, uvw_EmailQueue model);
    }
}
