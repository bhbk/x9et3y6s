using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

/*
 * https://andrewlock.net/custom-authorisation-policies-and-requirements-in-asp-net-core/
 */

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public class AuthorizeAlertAdmins : AuthorizationHandler<AuthorizeAlertAdminsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeAlertAdminsRequirement requirement)
        {
            if (context.User.HasClaim(x => x.Type == ClaimTypes.Role && x.Value == "Bhbk.WebApi.Alert(Admins)")
                && context.User.HasClaim(x => x.Type == ClaimTypes.System && x.Value == ClientType.user_agent.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class AuthorizeAlertAdminsRequirement : IAuthorizationRequirement
    {
        public AuthorizeAlertAdminsRequirement() { }
    }
}
