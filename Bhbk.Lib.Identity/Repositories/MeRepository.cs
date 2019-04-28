using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repositories
{
    public class MeRepository
    {
        private readonly IConfigurationRoot _conf;
        private readonly InstanceContext _instance;
        private readonly HttpClient _client;

        public MeRepository(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _conf = conf ?? throw new ArgumentNullException();
            _instance = instance;

            if (instance == InstanceContext.DeployedOrLocal || instance == InstanceContext.IntegrationTest)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _client = new HttpClient(connect);
            }

            if (instance == InstanceContext.UnitTest)
                _client = client;

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HttpResponseMessage> Info_DeleteCodesV1(string jwt)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/code/revoke";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Info_DeleteCodeV1(string jwt, Guid codeID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/code/" + codeID.ToString() + "/revoke";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Info_DeleteRefreshesV1(string jwt)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/refresh/revoke";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Info_DeleteRefreshV1(string jwt, Guid refreshID)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/refresh/" + refreshID.ToString() + "/revoke";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Info_GetQOTDV1()
        {
            var endpoint = "/info/v1/quote-of-the-day";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Info_GetV1(string jwt)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Info_GetCodesV1(string jwt)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/code";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Info_GetRefreshesV1(string jwt)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/refresh";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Info_SetPasswordV1(string jwt, UserAddPassword model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/info/v1/set-password";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Info_SetTwoFactorV1(string jwt, bool statusValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(statusValue), Encoding.UTF8, "application/json");
            var endpoint = "/info/v1/set-two-factor";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Info_UpdateV1(string jwt, UserModel model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/info/v1";

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Info_UpdateCodeV1(string jwt, string codeValue, string actionValue)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/code/" + codeValue + "/" + actionValue;

            if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                return await _client.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.UnitTest)
                return await _client.GetAsync(endpoint);

            throw new NotSupportedException();
        }
    }
}
