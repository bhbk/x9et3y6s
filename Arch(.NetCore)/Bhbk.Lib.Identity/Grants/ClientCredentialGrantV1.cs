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
    public class ClientCredentialGrantV1 : IOAuth2JwtGrant
    {
        private readonly IConfiguration _conf;
        private readonly HttpClient _http;
        private JwtSecurityToken _access, _refresh;

        public ClientCredentialGrantV1(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public ClientCredentialGrantV1(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            _conf = conf;

            if (instance == InstanceContext.DeployedOrLocal
                || instance == InstanceContext.End2EndTest)
            {
                var connect = new HttpClientHandler();

                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };
                connect.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

                _http = new HttpClient(connect);
                _http.BaseAddress = new Uri($"{_conf["IdentityStsUrls:BaseApiUrl"]}/{_conf["IdentityStsUrls:BaseApiPath"]}/");
            }
            else
                _http = http;

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
                            new KeyValuePair<string, string>("issuer_id", _conf["IdentityCredentials:IssuerName"]),
                            new KeyValuePair<string, string>("client_id", _conf["IdentityCredentials:AudienceName"]),
                            new KeyValuePair<string, string>("grant_type", "refresh_token"),
                            new KeyValuePair<string, string>("refresh_token", _refresh.RawData),
                        });

                response = await _http.PostAsync("oauth2/v1/ccg-rt", content);

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
                            new KeyValuePair<string, string>("issuer_id", _conf["IdentityCredentials:IssuerName"]),
                            new KeyValuePair<string, string>("client_id", _conf["IdentityCredentials:AudienceName"]),
                            new KeyValuePair<string, string>("grant_type", "client_secret"),
                            new KeyValuePair<string, string>("client_secret", _conf["IdentityCredentials:AudienceSecret"]),
                        });

                response = await _http.PostAsync("oauth2/v1/ccg", content);

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
