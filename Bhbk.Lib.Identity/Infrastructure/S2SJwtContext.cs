using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Primitives.Enums;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class S2SJwtContext : IS2SJwtContext
    {
        private readonly IConfigurationRoot _conf;
        private readonly ContextType _context;
        private readonly S2SClient _client;
        private static JwtSecurityToken _access, _refresh;

        public S2SJwtContext(IConfigurationRoot conf, ContextType context)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _conf = conf;
            _context = context;
            _client = new S2SClient(conf, context);
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
                    var result = _client.StsRefreshTokenV2(_conf["IdentityLogin:ClientName"],
                        new List<string> { _conf["IdentityLogin:AudienceName"] }, _refresh.RawData).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)content["access_token"]);
                        _refresh = new JwtSecurityToken((string)content["refresh_token"]);

                        Log.Information(typeof(S2SJwtContext).Name + " success using JWT refresh_token on " + DateTime.Now.ToString()
                            + ". JWT \"access_token\" valid from:" + _access.ValidFrom.ToLocalTime().ToString() + " to:" + _access.ValidTo.ToLocalTime().ToString()
                            + ". JWT \"refresh_token\" valid from:" + _refresh.ValidFrom.ToLocalTime().ToString() + " to:" + _refresh.ValidTo.ToLocalTime().ToString()+ ".");

                        return _access;
                    }

                    Log.Error(typeof(S2SJwtContext).Name + " fail using JWT refresh_token on " + DateTime.Now.ToString()
                        + ". Request Method:" + result.RequestMessage.Method.ToString() + ", URI:" + result.RequestMessage.RequestUri
                        + ". Response Code:" + (int)result.StatusCode + " " + result.StatusCode.ToString() + ", Reason:" + result.ReasonPhrase + ".");

                    throw new UnauthorizedAccessException();
                }

                else
                {
                    var result = _client.StsAccessTokenV2(_conf["IdentityLogin:ClientName"],
                        new List<string> { _conf["IdentityLogin:AudienceName"] }, _conf["IdentityLogin:UserName"], _conf["IdentityLogin:UserPass"]).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)content["access_token"]);
                        _refresh = new JwtSecurityToken((string)content["refresh_token"]);

                        Log.Information(typeof(S2SJwtContext).Name + " success using user/pass for JWT access_token on " + DateTime.Now.ToString()
                            + ". JWT \"access_token\" valid from:" + _access.ValidFrom.ToLocalTime().ToString() + " to:" + _access.ValidTo.ToLocalTime().ToString()
                            + ". JWT \"refresh_token\" valid from:" + _refresh.ValidFrom.ToLocalTime().ToString() + " to:" + _refresh.ValidTo.ToLocalTime().ToString() + ".");

                        return _access;
                    }

                    Log.Error(typeof(S2SJwtContext).Name + " fail using user/pass for JWT access_token on " + DateTime.Now.ToString()
                        + ". Request Method:" + result.RequestMessage.Method.ToString() + ", URI:" + result.RequestMessage.RequestUri
                        + ". Response Code:" + (int)result.StatusCode + " " + result.StatusCode.ToString() + ", Reason:" + result.ReasonPhrase + ".");

                    throw new UnauthorizedAccessException();
                }
            }
        }
    }
}
