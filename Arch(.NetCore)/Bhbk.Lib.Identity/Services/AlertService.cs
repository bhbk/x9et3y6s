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
        private readonly IOAuth2JwtGrant _ropg;
        private readonly AlertRepository _http;

        public AlertService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public AlertService(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            _ropg = new ResourceOwnerGrantV2(conf, instance, http);
            _http = new AlertRepository(conf, instance, http);
        }

        public JwtSecurityToken Jwt
        {
            get { return _ropg.AccessToken; }
            set { _ropg.AccessToken = value; }
        }

        public AlertRepository Http
        {
            get { return _http; }
        }

        public async ValueTask<bool> Email_EnqueueV1(EmailV1 model)
        {
            var response = await Http.Enqueue_EmailV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Text_EnqueueV1(TextV1 model)
        {
            var response = await Http.Enqueue_TextV1(_ropg.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
