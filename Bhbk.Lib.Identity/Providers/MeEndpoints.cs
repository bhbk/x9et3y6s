using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Providers
{
    public class MeClient : AdminEndpoints
    {
        public MeClient(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
            : base(conf, instance, client) { }
    }

    public class MeEndpoints
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly InstanceContext _instance;
        protected readonly HttpClient _client;

        public MeEndpoints(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
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

        public async Task<HttpResponseMessage> Detail_GetV1(string jwt)
        {
            var endpoint = "/me/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }
    }
}
