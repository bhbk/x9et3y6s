using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.Lib.Identity.Helpers
{
    public class S2STests : S2SHelper
    {
        public S2STests(IConfigurationRoot conf, IIdentityContext ioc, TestServer connect)
            : base(conf, ioc, connect) { }
    }

    public class S2SClients : S2SHelper
    {
        public S2SClients(IConfigurationRoot conf, IIdentityContext ioc, HttpClientHandler connect)
            : base(conf, ioc, connect) { }

        public async Task<HttpResponseMessage> PostEmailV1(JwtSecurityToken jwt, UserCreateEmail email)
        {
            if (_ioc.ContextStatus == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApis:AdminUrl"], _conf["IdentityApis:AdminPath"]));
                _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new StringContent(JsonConvert.SerializeObject(email), Encoding.UTF8, "application/json");

            return await _connect.PostAsync("/notify/v1/email", content);
        }

        public async Task<HttpResponseMessage> PostTextV1(JwtSecurityToken jwt, UserCreateText email)
        {
            if (_ioc.ContextStatus == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApis:AdminUrl"], _conf["IdentityApis:AdminPath"]));
                _connect.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt.RawData);
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new StringContent(JsonConvert.SerializeObject(email), Encoding.UTF8, "application/json");

            return await _connect.PostAsync("/notify/v1/text", content);
        }
    }

    //https://oauth.com/playground/
    public class S2SHelper
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly IIdentityContext _ioc;
        protected readonly HttpClient _connect;

        public S2SHelper(IConfigurationRoot conf, IIdentityContext ioc, TestServer connect)
        {
            _conf = conf;
            _ioc = ioc;
            _connect = connect.CreateClient();
        }

        public S2SHelper(IConfigurationRoot conf, IIdentityContext ioc, HttpClientHandler connect)
        {
            _conf = conf;
            _ioc = ioc;

            //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
            connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

            _connect = new HttpClient(connect);
        }

        //https://oauth.net/2/grant-types/password/
        public async Task<HttpResponseMessage> AccessTokenV1(string client, string audience, string user, string secret)
        {
            if(_ioc.ContextStatus == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApis:StsUrl"], _conf["IdentityApis:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("audience_id", audience),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", user),
                    new KeyValuePair<string, string>("password", secret),
                });

            return await _connect.PostAsync("/oauth/v1/access", content);
        }

        public async Task<HttpResponseMessage> AccessTokenV2(string client, List<string> audiences, string user, string secret)
        {
            if (_ioc.ContextStatus == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApis:StsUrl"], _conf["IdentityApis:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("audience", string.Join(",", audiences.Select(x => x))),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("user", user),
                    new KeyValuePair<string, string>("password", secret),
                });

            return await _connect.PostAsync("/oauth/v2/access", content);
        }

        //https://oauth.net/2/grant-types/authorization-code/
        public async Task<HttpResponseMessage> AuthorizationCodeRequestV2(string client, string audience, string user, string redirectUri, string scope)
        {
            if (_ioc.ContextStatus == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApis:StsUrl"], _conf["IdentityApis:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            string content = HttpUtility.UrlPathEncode("?client=" + client
                + "&audience=" + audience
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&response_type=" + "code"
                + "&scope=" + scope);

            return await _connect.GetAsync("/oauth/v2/authorization-code" + content);
        }

        public async Task<HttpResponseMessage> AuthorizationCodeV2(string client, string user, string redirectUri, string code)
        {
            if (_ioc.ContextStatus == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApis:StsUrl"], _conf["IdentityApis:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = HttpUtility.UrlPathEncode("?client=" + client
                + "&user=" + user
                + "&redirect_uri=" + redirectUri
                + "&grant_type=" + "code"
                + "&code=" + code);

            return await _connect.GetAsync("/oauth/v2/authorization" + content);
        }

        //https://oauth.net/2/grant-types/client-credentials/
        public async Task<HttpResponseMessage> ClientCredentialsV2(string client, string secret)
        {
            if (_ioc.ContextStatus == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApis:StsUrl"], _conf["IdentityApis:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "client_secret")
                });

            return await _connect.PostAsync("/oauth/v2/client", content);
        }

        //https://oauth.net/2/grant-types/refresh-token/
        public async Task<HttpResponseMessage> RefreshTokenV1(string client, string audience, string refresh)
        {
            if (_ioc.ContextStatus == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApis:StsUrl"], _conf["IdentityApis:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", client),
                    new KeyValuePair<string, string>("audience_id", audience),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                });

            return await _connect.PostAsync("/oauth/v1/refresh", content);
        }

        public async Task<HttpResponseMessage> RefreshTokenV2(string client, List<string> audiences, string refresh)
        {
            if (_ioc.ContextStatus == ContextType.Live)
            {
                _connect.BaseAddress = new Uri(string.Format("{0}{1}", _conf["IdentityApis:StsUrl"], _conf["IdentityApis:StsPath"]));
                _connect.DefaultRequestHeaders.Accept.Clear();
                _connect.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client", client),
                    new KeyValuePair<string, string>("audience", string.Join(",", audiences.Select(x => x))),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refresh),
                });

            return await _connect.PostAsync("/oauth/v2/refresh", content);
        }
    }
}
