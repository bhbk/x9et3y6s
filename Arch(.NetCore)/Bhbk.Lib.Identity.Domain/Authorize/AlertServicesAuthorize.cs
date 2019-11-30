using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

/*
 * https://andrewlock.net/custom-authorisation-policies-and-requirements-in-asp-net-core/
 */

namespace Bhbk.Lib.Identity.Domain.Authorize
{
    public class AlertServicesAuthorize : AuthorizationHandler<AlertServicesAuthorizeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AlertServicesAuthorizeRequirement requirement)
        {
            if (context.User.HasClaim(x => x.Type == ClaimTypes.Role && x.Value == "Bhbk.WebApi.Alert(Services)")
                && context.User.HasClaim(x => x.Type == ClaimTypes.System && x.Value == AudienceType.user_agent.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class AlertServicesAuthorizeRequirement : IAuthorizationRequirement
    {
        public AlertServicesAuthorizeRequirement() { }
    }
}
