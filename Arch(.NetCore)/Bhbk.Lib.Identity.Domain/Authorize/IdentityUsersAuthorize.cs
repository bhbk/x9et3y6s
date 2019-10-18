using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

/*
 * https://andrewlock.net/custom-authorisation-policies-and-requirements-in-asp-net-core/
 */

namespace Bhbk.Lib.Identity.Domain.Authorize
{
    public class IdentityUsersAuthorize : AuthorizationHandler<IdentityUsersAuthorizeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IdentityUsersAuthorizeRequirement requirement)
        {
            if (context.User.HasClaim(x => x.Type == ClaimTypes.Role && (x.Value == "(Built-In) Admins" 
                    || x.Value == "(Built-In) Services" 
                    || x.Value == "(Built-In) Users"))
                && context.User.HasClaim(x => x.Type == ClaimTypes.System && x.Value == ClientType.user_agent.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    } 

    public class IdentityUsersAuthorizeRequirement : IAuthorizationRequirement
    {
        public IdentityUsersAuthorizeRequirement() { }
    }
}
