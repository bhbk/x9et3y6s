using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

/*
 * https://andrewlock.net/custom-authorisation-policies-and-requirements-in-asp-net-core/
 */

namespace Bhbk.Lib.Identity.Domain.Authorize
{
    public class IdentityAdminsAuthorize : AuthorizationHandler<IdentityAdminsAuthorizeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IdentityAdminsAuthorizeRequirement requirement)
        {
            if (context.User.HasClaim(x => x.Type == ClaimTypes.Role && x.Value == "(Built-In) Admins")
                && context.User.HasClaim(x => x.Type == ClaimTypes.System && x.Value == ClientType.user_agent.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class IdentityAdminsAuthorizeRequirement : IAuthorizationRequirement
    {
        public IdentityAdminsAuthorizeRequirement() { }
    }
}
