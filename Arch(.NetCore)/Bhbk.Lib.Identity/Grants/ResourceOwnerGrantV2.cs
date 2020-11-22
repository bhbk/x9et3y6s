using Bhbk.Lib.Common.Primitives.Enums;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Grants
{
    public class ResourceOwnerGrantV2 : IOAuth2JwtGrant
    {
        private readonly HttpClient _http;
        private JwtSecurityToken _access, _refresh;
        private readonly string _issuerName, _audienceNames, _userName, _userPass;

        public ResourceOwnerGrantV2()
            : this(InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public ResourceOwnerGrantV2(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public ResourceOwnerGrantV2(InstanceContext instance, HttpClient http)
            : this(new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build(),
                  instance, http)
        { }

        public ResourceOwnerGrantV2(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            if (instance == InstanceContext.DeployedOrLocal
                || instance == InstanceContext.End2EndTest)
            {
                var connect = new HttpClientHandler();

                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };
                connect.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

                _http = new HttpClient(connect);
                _http.BaseAddress = new Uri($"{conf["IdentityStsUrls:BaseApiUrl"]}/{conf["IdentityStsUrls:BaseApiPath"]}/");
            }
            else
                _http = http;

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _issuerName = conf["IdentityCredentials:IssuerName"];
            _audienceNames = conf["IdentityCredentials:AudienceName"];
            _userName = conf["IdentityCredentials:UserName"];
            _userPass = conf["IdentityCredentials:UserPass"];
        }

        public JwtSecurityToken AccessToken
        {
            get { return GetAsync().Result; }
            set { _access = value; }
        }

        public ValueTask<JwtSecurityToken> AccessTokenAsync
        {
            get { return GetAsync(); }
            set { _access = value.Result; }
        }

        private async ValueTask<JwtSecurityToken> GetAsync()
        {
            //check if access is valid...
            if (_access != null
                && _access.ValidFrom < DateTime.UtcNow
                && _access.ValidTo > DateTime.UtcNow.AddSeconds(-60))
            {
                return _access;
            }
            //check if refresh is valid. update access with refresh if so.
            else if (_refresh != null
                && _refresh.ValidFrom < DateTime.UtcNow
                && _refresh.ValidTo > DateTime.UtcNow.AddSeconds(-60))
            {
                HttpResponseMessage response;
                FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                    {
                            new KeyValuePair<string, string>("issuer", _issuerName),
                            new KeyValuePair<string, string>("client", string.Join(",", new List<string> { _audienceNames })),
                            new KeyValuePair<string, string>("grant_type", "refresh_token"),
                            new KeyValuePair<string, string>("refresh_token", _refresh.RawData),
                        });

                response = await _http.PostAsync("oauth2/v2/ropg-rt", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

                    _access = new JwtSecurityToken((string)result["access_token"]);

                    if ((string)result["refresh_token"] != null)
                        _refresh = new JwtSecurityToken((string)result["refresh_token"]);

                    return _access;
                }

                throw new HttpRequestException(response.RequestMessage.ToString(),
                    new Exception(response.ToString()));
            }

            else
            {
                HttpResponseMessage response;
                FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                    {
                            new KeyValuePair<string, string>("issuer", _issuerName),
                            new KeyValuePair<string, string>("client", string.Join(",", new List<string> { _audienceNames })),
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("user", _userName),
                            new KeyValuePair<string, string>("password", _userPass),
                        });

                response = await _http.PostAsync("oauth2/v2/ropg", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(await response.Content.ReadAsStringAsync());

                    _access = new JwtSecurityToken((string)result["access_token"]);

                    if ((string)result["refresh_token"] != null)
                        _refresh = new JwtSecurityToken((string)result["refresh_token"]);

                    return _access;
                }

                throw new HttpRequestException(response.RequestMessage.ToString(),
                    new Exception(response.ToString()));
            }
        }
    }
}
