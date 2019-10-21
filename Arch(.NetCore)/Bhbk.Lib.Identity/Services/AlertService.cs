using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public class AlertService : IAlertService
    {
        private readonly ResourceOwnerGrant _ropg;
        private readonly AlertRepository _http;

        public AlertService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public AlertService(IConfiguration conf, InstanceContext instance, HttpClient client)
        {
            _ropg = new ResourceOwnerGrant(instance, client);
            _http = new AlertRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _ropg.RopgV2; }
            set { _ropg.RopgV2 = value; }
        }

        public AlertRepository Http
        {
            get { return _http; }
        }

        public async ValueTask<bool> Email_EnqueueV1(EmailCreate model)
        {
            var response = await Http.Enqueue_EmailV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Text_EnqueueV1(TextCreate model)
        {
            var response = await Http.Enqueue_TextV1(_ropg.RopgV2.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
