using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Identity.Models;
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

namespace Bhbk.Lib.Identity.Providers
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            throw new NotImplementedException();
        }
    }

    public class ClaimsProvider : IUserClaimsPrincipalFactory<AppUser>
    {
        private readonly FileInfo _lib = SearchRoots.ByAssemblyContext("appsettings-lib.json");
        private readonly IConfigurationRoot _conf;
        private readonly UserManager<AppUser> _userMgmt;

        public ClaimsProvider(UserManager<AppUser> userMgmt)
        {
            if (userMgmt == null)
                throw new ArgumentNullException();

            _conf = new ConfigurationBuilder()
                .SetBasePath(_lib.DirectoryName)
                .AddJsonFile(_lib.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _userMgmt = userMgmt;
        }

        public async Task<ClaimsPrincipal> CreateAsync(AppUser user)
        {
            var claims = new List<Claim>();

            foreach (string role in (await _userMgmt.GetRolesAsync(user)).ToList().OrderBy(x => x))
                claims.Add(new Claim(ClaimTypes.Role, role));

            //check if claims compatibility enabled. means pack claim(s) with old name too.
            if (bool.Parse(_conf["IdentityDefaults:CompatibilityModeClaims"]))
                foreach (var role in claims.Where(x => x.Type == ClaimTypes.Role).ToList().OrderBy(x => x.Type))
                    claims.Add(new Claim("role", role.Value, ClaimTypes.Role));

            foreach (Claim claim in (await _userMgmt.GetClaimsAsync(user)).ToList().OrderBy(x => x.Type))
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

        public async Task<ClaimsPrincipal> CreateRefreshAsync(AppUser user)
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
