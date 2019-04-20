using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Alert;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Providers
{
    public class AlertClient : AlertEndpoints
    {
        public AlertClient(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
            : base(conf, instance, client) { }
    }

    public class AlertEndpoints
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly InstanceContext _instance;
        protected readonly HttpClient _client;

        public AlertEndpoints(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _instance = instance;
            _conf = conf;

            if (instance == InstanceContext.DeployedOrLocal)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _client = new HttpClient(connect);
            }

            if (instance == InstanceContext.Testing)
                _client = client;
        }

        public async Task<HttpResponseMessage> Enqueue_EmailV1(string jwt, EmailCreate model)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/email/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["AlertUrls:BaseApiUrl"], _conf["AlertUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Enqueue_TextV1(string jwt, TextCreate model)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/text/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(
                    string.Format("{0}{1}{2}", _conf["AlertUrls:BaseApiUrl"], _conf["AlertUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.Testing)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }
    }
}
