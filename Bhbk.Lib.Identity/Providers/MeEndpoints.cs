using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Providers
{
    public class MeClient : AdminEndpoints
    {
        public MeClient(IConfigurationRoot conf, ExecutionType situation, HttpClient http)
            : base(conf, situation, http) { }
    }

    public class MeEndpoints
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ExecutionType _situation;
        protected readonly HttpClient _http;

        public MeEndpoints(IConfigurationRoot conf, ExecutionType situation, HttpClient http)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _situation = situation;
            _conf = conf;

            if (situation == ExecutionType.Live)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _http = new HttpClient(connect);
            }

            if (situation == ExecutionType.UnitTest)
                _http = http;
        }

        public async Task<HttpResponseMessage> Detail_GetV1(JwtSecurityToken jwt)
        {
            var endpoint = "/me/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.GetAsync(
                    string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_situation == ExecutionType.UnitTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }
    }
}
