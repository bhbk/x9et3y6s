using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public class AlertService : IAlertService
    {
        public IOAuth2JwtGrant Grant { get; set; }
        public AlertRepository Endpoints { get; }

        public AlertService()
            : this(InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public AlertService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public AlertService(InstanceContext instance, HttpClient http)
            : this(new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build(),
                  instance, http)
        { }

        public AlertService(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            Endpoints = new AlertRepository(conf, instance, http);
        }

        public async ValueTask<bool> Dequeue_EmailV1(Guid emailID)
        {
            var response = await Endpoints.Dequeue_EmailV1(Grant.AccessToken.RawData, emailID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Dequeue_TextV1(Guid textID)
        {
            var response = await Endpoints.Dequeue_TextV1(Grant.AccessToken.RawData, textID);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Enqueue_EmailV1(EmailV1 model)
        {
            var response = await Endpoints.Enqueue_EmailV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<bool> Enqueue_TextV1(TextV1 model)
        {
            var response = await Endpoints.Enqueue_TextV1(Grant.AccessToken.RawData, model);

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }
    }
}
