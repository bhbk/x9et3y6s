using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Providers
{
    public class MeClient : AdminEndpoints
    {
        public MeClient(IConfigurationRoot conf, ContextType situation, HttpClient http)
            : base(conf, situation, http) { }
    }

    public class MeTester : AdminEndpoints
    {
        public MeTester(IConfigurationRoot conf, TestServer server)
            : base(conf, server) { }
    }

    public class MeEndpoints
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ContextType _situation;
        protected readonly HttpClient _http;

        public MeEndpoints(IConfigurationRoot conf, ContextType situation, HttpClient http)
        {
            if (conf == null)
                throw new ArgumentNullException();

            var connect = new HttpClientHandler();

            //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
            connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

            _situation = situation;
            _conf = conf;
            _http = http;
        }

        public MeEndpoints(IConfigurationRoot conf, TestServer server)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _situation = ContextType.UnitTest;
            _conf = conf;
            _http = server.CreateClient();
        }

        public async Task<HttpResponseMessage> Detail_GetV1(JwtSecurityToken jwt)
        {
            var endpoint = "/me/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ContextType.UnitTest)
                return await _http.GetAsync(endpoint);

            if (_situation == ContextType.IntegrationTest || _situation == ContextType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            throw new NotSupportedException();
        }
    }
}
