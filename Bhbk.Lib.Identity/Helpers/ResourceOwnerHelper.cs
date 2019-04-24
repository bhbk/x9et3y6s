using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Bhbk.Lib.Identity.Helpers
{
    public class ResourceOwnerHelper : IResourceOwnerHelper
    {
        private readonly IConfigurationRoot _conf;
        private readonly InstanceContext _instance;
        private readonly HttpClient _client;
        private JwtSecurityToken _access, _refresh;

        public ResourceOwnerHelper(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _conf = conf ?? throw new ArgumentNullException();
            _instance = instance;

            if (instance == InstanceContext.DeployedOrLocal)
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

        public JwtSecurityToken JwtV1
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

                    var endpoint = "/oauth2/v1/ropg-rt";

                    if (_instance == InstanceContext.DeployedOrLocal)
                        response = _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content).Result;

                    else if (_instance == InstanceContext.UnitTest)
                        response = _client.PostAsync(endpoint, content).Result;

                    else
                        throw new NotImplementedException();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)result["access_token"]);
                        _refresh = new JwtSecurityToken((string)result["refresh_token"]);

                        return _access;
                    }

                    throw new HttpRequestException(response.RequestMessage
                        + Environment.NewLine + response);
                }

                else
                {
                    HttpResponseMessage response = null;
                    FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("issuer_id", _conf["IdentityLogin:IssuerName"]),
                            new KeyValuePair<string, string>("client_id", _conf["IdentityLogin:ClientName"]),
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("username", _conf["IdentityLogin:UserName"]),
                            new KeyValuePair<string, string>("password", _conf["IdentityLogin:UserPass"]),
                        });

                    var endpoint = "/oauth2/v1/ropg";

                    if (_instance == InstanceContext.DeployedOrLocal)
                        response = _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content).Result;

                    else if (_instance == InstanceContext.UnitTest)
                        response = _client.PostAsync(endpoint, content).Result;

                    else
                        throw new NotImplementedException();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)result["access_token"]);
                        _refresh = new JwtSecurityToken((string)result["refresh_token"]);

                        return _access;
                    }

                    throw new HttpRequestException(response.RequestMessage
                        + Environment.NewLine + response);
                }
            }
            set { _access = value; }
        }

        public JwtSecurityToken JwtV2
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

                    var endpoint = "/oauth2/v2/ropg-rt";

                    if (_instance == InstanceContext.DeployedOrLocal)
                        response = _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content).Result;

                    else if (_instance == InstanceContext.UnitTest)
                        response = _client.PostAsync(endpoint, content).Result;

                    else
                        throw new NotImplementedException();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)result["access_token"]);
                        _refresh = new JwtSecurityToken((string)result["refresh_token"]);

                        return _access;
                    }

                    throw new HttpRequestException(response.RequestMessage
                        + Environment.NewLine + response);
                }

                else
                {
                    HttpResponseMessage response = null;
                    FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("issuer", _conf["IdentityLogin:IssuerName"]),
                            new KeyValuePair<string, string>("client", string.Join(",", new List<string> { _conf["IdentityLogin:ClientName"] })),
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("user", _conf["IdentityLogin:UserName"]),
                            new KeyValuePair<string, string>("password", _conf["IdentityLogin:UserPass"]),
                        });

                    var endpoint = "/oauth2/v2/ropg";

                    if (_instance == InstanceContext.DeployedOrLocal)
                        response = _client.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content).Result;

                    else if (_instance == InstanceContext.UnitTest)
                        response = _client.PostAsync(endpoint, content).Result;

                    else
                        throw new NotImplementedException();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)result["access_token"]);
                        _refresh = new JwtSecurityToken((string)result["refresh_token"]);

                        return _access;
                    }

                    throw new HttpRequestException(response.RequestMessage
                        + Environment.NewLine + response);
                }
            }
            set { _access = value; }
        }
    }
}
