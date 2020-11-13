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
        public AlertRepository Http { get; }
        public IOAuth2JwtGrant Grant { get; set; }

        public AlertService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public AlertService(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            Http = new AlertRepository(conf, instance, http);
        }

        public JwtSecurityToken Jwt
        {
            get { return Grant.Jwt; }
            set { Grant.Jwt = value; }
        }

        public async ValueTask<bool> Dequeue_EmailV1(Guid emailID)
        {
            var response = await Http.Dequeue_EmailV1(Grant.Jwt.RawData, emailID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Dequeue_TextV1(Guid textID)
        {
            var response = await Http.Dequeue_TextV1(Grant.Jwt.RawData, textID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Enqueue_EmailV1(EmailV1 model)
        {
            var response = await Http.Enqueue_EmailV1(Grant.Jwt.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<bool> Enqueue_TextV1(TextV1 model)
        {
            var response = await Http.Enqueue_TextV1(Grant.Jwt.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
