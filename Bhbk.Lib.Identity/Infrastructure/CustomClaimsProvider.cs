using Bhbk.Lib.Identity.Managers;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class CustomClaimsProvider : IUserClaimsPrincipalFactory<AppUser>
    {
        private readonly UserManager<AppUser> _ioc;
        private readonly ConfigManager _conf;

        public CustomClaimsProvider(UserManager<AppUser> ioc, ConfigManager conf)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _ioc = ioc;
            _conf = conf;
        }

        public async Task<ClaimsPrincipal> CreateAsync(AppUser user)
        {
            var claims = new List<Claim>();

            foreach (string role in await _ioc.GetRolesAsync(user))
                claims.Add(new Claim(ClaimTypes.Role, role));

            foreach (Claim claim in await _ioc.GetClaimsAsync(user))
                claims.Add(claim);

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now)
                .ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.Now)
                .ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now)
                .ToUniversalTime().Add(new TimeSpan((int)_conf.Tweaks.DefaultAccessTokenLife)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }
    }
}
