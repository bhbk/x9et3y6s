using Bhbk.Lib.Identity.Internal.EntityModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Providers
{
    public class ClientClaimFactory
    {
        private readonly IConfigurationRoot _conf;

        public ClientClaimFactory(IConfigurationRoot conf)
        {
            _conf = conf;
        }

        public async Task<ClaimsPrincipal> CreateAsync(AppClient client)
        {
            var claims = new List<Claim>();

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow)
                .Add(new TimeSpan(UInt32.Parse(_conf["IdentityDefaults:AccessTokenExpire"]))).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }
    }
}
