using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public class UserPolicyHandler : AuthorizationHandler<UserPolicyRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserPolicyRequirement requirement)
        {
            if(context.User.Claims.Where(x => x.Type == ClaimTypes.Role
                && x.Value == "Bhbk.WebApi.Identity(Users)").Any())
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
