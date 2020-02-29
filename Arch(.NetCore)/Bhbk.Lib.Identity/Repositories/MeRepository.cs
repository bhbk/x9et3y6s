using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
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
        private readonly IConfiguration _conf;
        private readonly InstanceContext _instance;
        private readonly HttpClient _http;

        public MeRepository(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public MeRepository(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            _conf = conf;
            _instance = instance;

            if (instance == InstanceContext.DeployedOrLocal)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _http = new HttpClient(connect);
            }
            else if (instance == InstanceContext.End2EndTest)
                _http = http;
            else
                throw new NotImplementedException();

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async ValueTask<HttpResponseMessage> Info_DeleteCodesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/code/revoke";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Info_DeleteCodeV1(string jwt, Guid codeID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/code/" + codeID.ToString() + "/revoke";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Info_DeleteRefreshesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/refresh/revoke";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Info_DeleteRefreshV1(string jwt, Guid refreshID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/refresh/" + refreshID.ToString() + "/revoke";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.DeleteAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.DeleteAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Info_GetMOTDV1()
        {
            var endpoint = "/info/v1/msg-of-the-day";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Info_GetV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Info_GetCodesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/code";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Info_GetRefreshesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/refresh";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Info_SetPasswordV1(string jwt, PasswordAddV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/info/v1/set-password";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Info_SetTwoFactorV1(string jwt, bool statusValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(statusValue), Encoding.UTF8, "application/json");
            var endpoint = "/info/v1/set-two-factor";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Info_UpdateV1(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/info/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PutAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PutAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Info_UpdateCodeV1(string jwt, string codeValue, string actionValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var endpoint = "/info/v1/code/" + codeValue + "/" + actionValue;

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.GetAsync(string.Format("{0}{1}{2}", _conf["IdentityMeUrls:BaseApiUrl"], _conf["IdentityMeUrls:BaseApiPath"], endpoint));

            if (_instance == InstanceContext.End2EndTest)
                return await _http.GetAsync(endpoint);

            throw new NotSupportedException();
        }

        public async ValueTask<HttpResponseMessage> Init_GetAudiencesV1(string jwt, PageStateTypeC model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/init/v1/page";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityAdminUrls:BaseApiUrl"], _conf["IdentityAdminUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.End2EndTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }
    }
}
