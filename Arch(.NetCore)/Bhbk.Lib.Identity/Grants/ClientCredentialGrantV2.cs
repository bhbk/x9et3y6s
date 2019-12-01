﻿using Bhbk.Lib.Common.Primitives.Enums;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Bhbk.Lib.Identity.Grants
{
    public class ClientCredentialGrantV2 : IOAuth2JwtGrant
    {
        private readonly IConfiguration _conf;
        private readonly InstanceContext _instance;
        private readonly HttpClient _http;
        private JwtSecurityToken _access, _refresh;

        public ClientCredentialGrantV2(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public ClientCredentialGrantV2(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            _conf = conf;
            _instance = instance;

            if (instance == InstanceContext.DeployedOrLocal
                || instance == InstanceContext.IntegrationTest)
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

        public JwtSecurityToken AccessToken
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
                            new KeyValuePair<string, string>("issuer", _conf["IdentityCredentials:IssuerName"]),
                            new KeyValuePair<string, string>("client", string.Join(",", new List<string> { _conf["IdentityCredentials:AudienceName"] })),
                            new KeyValuePair<string, string>("grant_type", "refresh_token"),
                            new KeyValuePair<string, string>("refresh_token", _refresh.RawData),
                        });

                    var endpoint = "/oauth2/v2/ccg-rt";

                    if (_instance == InstanceContext.DeployedOrLocal 
                        || _instance == InstanceContext.IntegrationTest)
                        response = _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content).Result;

                    else if (_instance == InstanceContext.UnitTest)
                        response = _http.PostAsync(endpoint, content).Result;

                    else
                        throw new NotImplementedException();

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
                            new KeyValuePair<string, string>("issuer", _conf["IdentityCredentials:IssuerName"]),
                            new KeyValuePair<string, string>("client", string.Join(",", new List<string> { _conf["IdentityCredentials:AudienceName"] })),
                            new KeyValuePair<string, string>("grant_type", "client_secret"),
                            new KeyValuePair<string, string>("client_secret", _conf["IdentityCredentials:AudienceSecret"]),
                        });

                    var endpoint = "/oauth2/v2/ccg";

                    if (_instance == InstanceContext.DeployedOrLocal 
                        || _instance == InstanceContext.IntegrationTest)
                        response = _http.PostAsync(string.Format("{0}{1}{2}", _conf["IdentityStsUrls:BaseApiUrl"], _conf["IdentityStsUrls:BaseApiPath"], endpoint), content).Result;

                    else if (_instance == InstanceContext.UnitTest)
                        response = _http.PostAsync(endpoint, content).Result;

                    else
                        throw new NotImplementedException();

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
            set { _access = value; }
        }
    }
}
