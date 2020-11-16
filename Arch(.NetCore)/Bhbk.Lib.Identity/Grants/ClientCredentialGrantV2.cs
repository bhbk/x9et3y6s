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
    public class ClientCredentialGrantV2 : IOAuth2JwtGrant
    {
        private readonly HttpClient _http;
        private JwtSecurityToken _access, _refresh;
        private readonly string _issuerName, _audienceNames, _audienceSecret;

        public ClientCredentialGrantV2()
            : this(InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public ClientCredentialGrantV2(InstanceContext instance, HttpClient http)
            : this(new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build(),
                  instance, http)
        { }

        public ClientCredentialGrantV2(IConfiguration conf, InstanceContext instance, HttpClient http)
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
            _audienceSecret = conf["IdentityCredentials:AudienceSecret"];
        }

        public JwtSecurityToken Jwt
        {
            get { return GetAsync().Result; }
            set { _access = value; }
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
                HttpResponseMessage response = null;
                FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                    {
                            new KeyValuePair<string, string>("issuer", _issuerName),
                            new KeyValuePair<string, string>("client", string.Join(",", new List<string> { _audienceNames })),
                            new KeyValuePair<string, string>("grant_type", "refresh_token"),
                            new KeyValuePair<string, string>("refresh_token", _refresh.RawData),
                        });

                response = await _http.PostAsync("oauth2/v2/ccg-rt", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    _access = new JwtSecurityToken((string)result["access_token"]);

                    if ((string)result["refresh_token"] != null)
                        _refresh = new JwtSecurityToken((string)result["refresh_token"]);

                    return _access;
                }

                throw new HttpRequestException(response.ToString(),
                    new Exception(response.RequestMessage.ToString()));
            }

            else
            {
                HttpResponseMessage response = null;
                FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                    {
                            new KeyValuePair<string, string>("issuer", _issuerName),
                            new KeyValuePair<string, string>("client", string.Join(",", new List<string> { _audienceNames })),
                            new KeyValuePair<string, string>("grant_type", "client_secret"),
                            new KeyValuePair<string, string>("client_secret", _audienceSecret),
                        });

                response = await _http.PostAsync("oauth2/v2/ccg", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    _access = new JwtSecurityToken((string)result["access_token"]);

                    if ((string)result["refresh_token"] != null)
                        _refresh = new JwtSecurityToken((string)result["refresh_token"]);

                    return _access;
                }

                throw new HttpRequestException(response.ToString(),
                    new Exception(response.RequestMessage.ToString()));
            }
        }
    }
}
