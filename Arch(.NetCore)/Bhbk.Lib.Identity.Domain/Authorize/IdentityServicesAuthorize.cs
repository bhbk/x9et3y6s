using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

/*
 * https://andrewlock.net/custom-authorisation-policies-and-requirements-in-asp-net-core/
 */

namespace Bhbk.Lib.Identity.Domain.Authorize
{
    public class IdentityServicesAuthorize : AuthorizationHandler<IdentityServicesAuthorizeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IdentityServicesAuthorizeRequirement requirement)
        {
            if (context.User.HasClaim(x => x.Type == ClaimTypes.Role && x.Value == "(Built-In) Services")
                && context.User.HasClaim(x => x.Type == ClaimTypes.System && x.Value == AudienceType.server.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class IdentityServicesAuthorizeRequirement : IAuthorizationRequirement
    {
        public IdentityServicesAuthorizeRequirement() { }
    }
}
