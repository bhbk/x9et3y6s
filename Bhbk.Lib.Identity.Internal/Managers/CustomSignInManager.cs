using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

//https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.signinmanager-1

namespace Bhbk.Lib.Identity.Managers
{
    //https://www.stevejgordon.co.uk/extending-the-asp-net-core-identity-signinmanager
    public partial class CustomSignInManager : SignInManager<AppUser>
    {
        public CustomSignInManager(UserManager<AppUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<AppUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<AppUser>> logger, IAuthenticationSchemeProvider schema)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schema)
        {

        }
    }
}
