using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public class MeServiceRepository
    {
        private readonly HttpClient _http;

        public MeServiceRepository(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            if (instance == InstanceContext.DeployedOrLocal
                || instance == InstanceContext.End2EndTest)
            {
                var connect = new HttpClientHandler();

                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };
                connect.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

                _http = new HttpClient(connect);
                _http.BaseAddress = new Uri($"{conf["IdentityMeUrls:BaseApiUrl"]}/{conf["IdentityMeUrls:BaseApiPath"]}/");
            }
            else
                _http = http;

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async ValueTask<HttpResponseMessage> Info_DeleteCodesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("info/v1/code/revoke");
        }

        public async ValueTask<HttpResponseMessage> Info_DeleteCodeV1(string jwt, Guid codeID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("info/v1/code/" + codeID.ToString() + "/revoke");
        }

        public async ValueTask<HttpResponseMessage> Info_DeleteRefreshesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("info/v1/refresh/revoke");
        }

        public async ValueTask<HttpResponseMessage> Info_DeleteRefreshV1(string jwt, Guid refreshID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("info/v1/refresh/" + refreshID.ToString() + "/revoke");
        }

        public async ValueTask<HttpResponseMessage> Info_GetMOTDV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("info/v1/msg-of-the-day");
        }

        public async ValueTask<HttpResponseMessage> Info_GetV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("info/v1");
        }

        public async ValueTask<HttpResponseMessage> Info_GetCodesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("info/v1/code");
        }

        public async ValueTask<HttpResponseMessage> Info_GetRefreshesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("info/v1/refresh");
        }

        public async ValueTask<HttpResponseMessage> Info_SetPasswordV1(string jwt, PasswordAddV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("info/v1/set-password",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Info_SetTwoFactorV1(string jwt, bool statusValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("info/v1/set-two-factor",
                new StringContent(JsonConvert.SerializeObject(statusValue), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Info_UpdateV1(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("info/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Info_UpdateCodeV1(string jwt, string codeValue, string actionValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("info/v1/code/" + codeValue + "/" + actionValue);
        }

        public async ValueTask<HttpResponseMessage> Init_GetAudiencesV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("init/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }
    }
}
