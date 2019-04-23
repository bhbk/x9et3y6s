using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

/*
 * https://andrewlock.net/custom-authorisation-policies-and-requirements-in-asp-net-core/
 */

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public class AuthorizeAlertUsers : AuthorizationHandler<AuthorizeAlertUsersRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeAlertUsersRequirement requirement)
        {
            if (context.User.HasClaim(x => x.Type == ClaimTypes.Role && (x.Value == "Bhbk.WebApi.Alert(Admins)"
                    || x.Value == "Bhbk.WebApi.Alert(Services)"
                    || x.Value == "Bhbk.WebApi.Alert(Users)"))
                && context.User.HasClaim(x => x.Type == ClaimTypes.System && x.Value == ClientType.user_agent.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class AuthorizeAlertUsersRequirement : IAuthorizationRequirement
    {
        public AuthorizeAlertUsersRequirement() { }
    }
}
