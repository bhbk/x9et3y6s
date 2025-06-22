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
    public class UserServiceRepository
    {
        private readonly HttpClient _http;

        public UserServiceRepository(IConfiguration conf, InstanceContext env, HttpClient http)
        {
            if (env == InstanceContext.DeployedOrLocal
                || env == InstanceContext.End2EndTest)
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

        #region Profile

        public async ValueTask<HttpResponseMessage> Profile_GetV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("profile/v1");
        }

        public async ValueTask<HttpResponseMessage> Profile_UpdateV1(string jwt, UserV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("profile/v1",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        #endregion

        #region Session

        public async ValueTask<HttpResponseMessage> Session_DeleteCodesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("session/v1/codes");
        }

        public async ValueTask<HttpResponseMessage> Session_DeleteCodeV1(string jwt, Guid codeID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("session/v1/codes/" + codeID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Session_DeleteRefreshesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("session/v1/refreshes");
        }

        public async ValueTask<HttpResponseMessage> Session_DeleteRefreshV1(string jwt, Guid refreshID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("session/v1/refreshes/" + refreshID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Session_GetCodesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("session/v1/codes");
        }

        public async ValueTask<HttpResponseMessage> Session_GetRefreshesV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("session/v1/refreshes");
        }

        public async ValueTask<HttpResponseMessage> Session_UpdateCodeV1(string jwt, string codeValue, string actionValue)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("session/v1/codes/" + codeValue + "/" + actionValue);
        }

        public async ValueTask<HttpResponseMessage> Session_LogoutV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("session/v1/logout", null);
        }

        #endregion

        #region Credentials

        public async ValueTask<HttpResponseMessage> Credentials_SetPasswordV1(string jwt, PasswordAddV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PutAsync("credentials/v1/password/set",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        #endregion

        #region MOTD

        public async ValueTask<HttpResponseMessage> MOTD_GetV1(string jwt)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.GetAsync("motd/v1");
        }

        #endregion

        #region Init

        public async ValueTask<HttpResponseMessage> Init_GetAudiencesV1(string jwt, DataStateV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("init/v1/page",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        #endregion
    }
}
