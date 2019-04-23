using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

/*
 * https://andrewlock.net/custom-authorisation-policies-and-requirements-in-asp-net-core/
 */

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public class AuthorizeIdentityServices : AuthorizationHandler<AuthorizeIdentityServicesRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeIdentityServicesRequirement requirement)
        {
            if (context.User.HasClaim(x => x.Type == ClaimTypes.Role && x.Value == "(Built-In) Services")
                && context.User.HasClaim(x => x.Type == ClaimTypes.System && x.Value == ClientType.server.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class AuthorizeIdentityServicesRequirement : IAuthorizationRequirement
    {
        public AuthorizeIdentityServicesRequirement() { }
    }
}
