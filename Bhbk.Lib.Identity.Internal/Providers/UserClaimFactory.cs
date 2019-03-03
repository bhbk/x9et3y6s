using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.Repository;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Providers
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.iuserclaimsprincipalfactory-1
     */

    public class UserClaimFactory
    {
        private readonly IConfigurationRoot _conf;
        private readonly UserRepository _repo;

        public UserClaimFactory(UserRepository repo, IConfigurationRoot conf)
        {
            if (repo == null)
                throw new ArgumentNullException();

            _repo = repo;
            _conf = conf;
        }

        public async Task<ClaimsPrincipal> CreateAsync(UserModel user)
        {
            var claims = new List<Claim>();

            foreach (string role in (await _repo.GetRolesAsync(user.Id)).ToList().OrderBy(x => x))
                claims.Add(new Claim(ClaimTypes.Role, role));

            //check if claims compatibility enabled. means pack claim(s) with old name too.
            if (bool.Parse(_conf["IdentityDefaults:CompatibilityModeClaims"]))
                foreach (var role in claims.Where(x => x.Type == ClaimTypes.Role).ToList().OrderBy(x => x.Type))
                    claims.Add(new Claim("role", role.Value, ClaimTypes.Role));

            foreach (Claim claim in (await _repo.GetClaimsAsync(user.Id)).ToList().OrderBy(x => x.Type))
                claims.Add(claim);

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

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

        public async Task<ClaimsPrincipal> CreateRefreshAsync(UserModel user)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            //not before timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //issued at timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            //expire on timestamp
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow)
                .Add(new TimeSpan(UInt32.Parse(_conf["IdentityDefaults:RefreshTokenExpire"]))).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var identity = new ClaimsIdentity(claims, "JWT");
            var result = new ClaimsPrincipal(identity);

            return await Task.Run(() => result);
        }
    }
}
