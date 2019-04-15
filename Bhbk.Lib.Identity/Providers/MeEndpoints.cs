using Bhbk.Lib.Core.UnitOfWork;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Providers
{
    public class MeClient : AdminEndpoints
    {
        public MeClient(IConfigurationRoot conf, ExecutionContext situation, HttpClient client)
            : base(conf, situation, client) { }
    }

    public class MeEndpoints
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ExecutionContext _situation;
        protected readonly HttpClient _client;

        public MeEndpoints(IConfigurationRoot conf, ExecutionContext situation, HttpClient client)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _situation = situation;
            _conf = conf;

            if (situation == ExecutionContext.DeployedOrLocal)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _client = new HttpClient(connect);
            }

            if (situation == ExecutionContext.Testing)
                _client = client;
        }

        public async Task<HttpResponseMessage> Detail_GetV1(string jwt)
        {
            var endpoint = "/me/v1";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionContext.DeployedOrLocal)
                return await _client.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionContext.Testing)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }
    }
}
