using Bhbk.Lib.Common.Primitives.Enums;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Bhbk.Lib.Identity.Grants
{
    public class ClientCredentialGrant : IClientCredentialGrant
    {
        private readonly IConfiguration _conf;
        private readonly InstanceContext _instance;
        private readonly HttpClient _http;
        private JwtSecurityToken _access, _refresh;

        public ClientCredentialGrant()
            : this(InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public ClientCredentialGrant(InstanceContext instance, HttpClient http)
        {
            _conf = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _instance = instance;

            if (instance == InstanceContext.DeployedOrLocal || instance == InstanceContext.IntegrationTest)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _http = new HttpClient(connect);
            }

            if (instance == InstanceContext.UnitTest)
                _http = http;

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public JwtSecurityToken CcgV1
        {
            get
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
                            new KeyValuePair<string, string>("issuer_id", _conf["IdentityLogin:IssuerName"]),
                            new KeyValuePair<string, string>("client_id", _conf["IdentityLogin:ClientName"]),
                            new KeyValuePair<string, string>("grant_type", "refresh_token"),
                            new KeyValuePair<string, string>("refresh_token", _refresh.RawData),
                        });

                    var endpoint = "/oauth2/v1/ccg-rt";

                    if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                        response = _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content).Result;

                    else if (_instance == InstanceContext.UnitTest)
                        response = _http.PostAsync(endpoint, content).Result;

                    else
                        throw new NotImplementedException();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)result["access_token"]);
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
                            new KeyValuePair<string, string>("issuer_id", _conf["IdentityLogin:IssuerName"]),
                            new KeyValuePair<string, string>("client_id", _conf["IdentityLogin:ClientName"]),
                            new KeyValuePair<string, string>("grant_type", "client_secret"),
                            new KeyValuePair<string, string>("client_secret", _conf["IdentityLogin:ClientSecret"]),
                        });

                    var endpoint = "/oauth2/v1/ccg";

                    if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                        response = _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content).Result;

                    else if (_instance == InstanceContext.UnitTest)
                        response = _http.PostAsync(endpoint, content).Result;

                    else
                        throw new NotImplementedException();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)result["access_token"]);
                        _refresh = new JwtSecurityToken((string)result["refresh_token"]);

                        return _access;
                    }

                    throw new HttpRequestException(response.ToString(),
                        new Exception(response.RequestMessage.ToString()));
                }
            }
            set { _access = value; }
        }

        public JwtSecurityToken CcgV2
        {
            get
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
                            new KeyValuePair<string, string>("issuer", _conf["IdentityLogin:IssuerName"]),
                            new KeyValuePair<string, string>("client", string.Join(",", new List<string> { _conf["IdentityLogin:ClientName"] })),
                            new KeyValuePair<string, string>("grant_type", "refresh_token"),
                            new KeyValuePair<string, string>("refresh_token", _refresh.RawData),
                        });

                    var endpoint = "/oauth2/v2/ccg-rt";

                    if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                        response = _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content).Result;

                    else if (_instance == InstanceContext.UnitTest)
                        response = _http.PostAsync(endpoint, content).Result;

                    else
                        throw new NotImplementedException();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)result["access_token"]);
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
                            new KeyValuePair<string, string>("issuer", _conf["IdentityLogin:IssuerName"]),
                            new KeyValuePair<string, string>("client", string.Join(",", new List<string> { _conf["IdentityLogin:ClientName"] })),
                            new KeyValuePair<string, string>("grant_type", "client_secret"),
                            new KeyValuePair<string, string>("client_secret", _conf["IdentityLogin:ClientSecret"]),
                        });

                    var endpoint = "/oauth2/v2/ccg";

                    if (_instance == InstanceContext.DeployedOrLocal || _instance == InstanceContext.IntegrationTest)
                        response = _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content).Result;

                    else if (_instance == InstanceContext.UnitTest)
                        response = _http.PostAsync(endpoint, content).Result;

                    else
                        throw new NotImplementedException();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)result["access_token"]);
                        _refresh = new JwtSecurityToken((string)result["refresh_token"]);

                        return _access;
                    }

                    throw new HttpRequestException(response.ToString(),
                        new Exception(response.RequestMessage.ToString()));
                }
            }
            set { _access = value; }
        }
    }
}
