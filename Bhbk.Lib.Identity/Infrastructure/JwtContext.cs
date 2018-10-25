using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class JwtContext : IJwtContext
    {
        private readonly IConfigurationRoot _conf;
        private readonly ContextType _situation;
        private readonly StsClient _sts;
        private static JwtSecurityToken _access, _refresh;

        public JwtContext(IConfigurationRoot conf, ContextType situation)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _conf = conf;
            _situation = situation;
            _sts = new StsClient(conf, situation);
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
                    var result = _sts.RefreshTokenV2(_conf["IdentityLogin:ClientName"],
                        new List<string> { _conf["IdentityLogin:AudienceName"] }, _refresh.RawData).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)content["access_token"]);
                        _refresh = new JwtSecurityToken((string)content["refresh_token"]);

                        Log.Information(typeof(JwtContext).Name + " success using JWT refresh_token on " + DateTime.Now.ToString()
                            + ". JWT \"access_token\" valid from:" + _access.ValidFrom.ToLocalTime().ToString() + " to:" + _access.ValidTo.ToLocalTime().ToString()
                            + ". JWT \"refresh_token\" valid from:" + _refresh.ValidFrom.ToLocalTime().ToString() + " to:" + _refresh.ValidTo.ToLocalTime().ToString()+ ".");

                        return _access;
                    }

                    Log.Error(typeof(JwtContext).Name + " fail using JWT refresh_token on " + DateTime.Now.ToString()
                        + ". Request Method:" + result.RequestMessage.Method.ToString() + ", URI:" + result.RequestMessage.RequestUri
                        + ". Response Code:" + (int)result.StatusCode + " " + result.StatusCode.ToString() + ", Reason:" + result.ReasonPhrase + ".");

                    throw new UnauthorizedAccessException();
                }

                else
                {
                    var result = _sts.AccessTokenV2(_conf["IdentityLogin:ClientName"],
                        new List<string> { _conf["IdentityLogin:AudienceName"] }, _conf["IdentityLogin:UserName"], _conf["IdentityLogin:UserPass"]).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                        _access = new JwtSecurityToken((string)content["access_token"]);
                        _refresh = new JwtSecurityToken((string)content["refresh_token"]);

                        Log.Information(typeof(JwtContext).Name + " success using user/pass for JWT access_token on " + DateTime.Now.ToString()
                            + ". JWT \"access_token\" valid from:" + _access.ValidFrom.ToLocalTime().ToString() + " to:" + _access.ValidTo.ToLocalTime().ToString()
                            + ". JWT \"refresh_token\" valid from:" + _refresh.ValidFrom.ToLocalTime().ToString() + " to:" + _refresh.ValidTo.ToLocalTime().ToString() + ".");

                        return _access;
                    }

                    Log.Error(typeof(JwtContext).Name + " fail using user/pass for JWT access_token on " + DateTime.Now.ToString()
                        + ". Request Method:" + result.RequestMessage.Method.ToString() + ", URI:" + result.RequestMessage.RequestUri
                        + ". Response Code:" + (int)result.StatusCode + " " + result.StatusCode.ToString() + ", Reason:" + result.ReasonPhrase + ".");

                    throw new UnauthorizedAccessException();
                }
            }
        }
    }
}
