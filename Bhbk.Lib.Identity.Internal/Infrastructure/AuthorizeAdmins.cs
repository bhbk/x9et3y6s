using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public class AuthorizeAdmins : AuthorizationHandler<AuthorizeAdminsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeAdminsRequirement requirement)
        {
            if (context.User.Claims.Where(x => x.Type == ClaimTypes.Role
                 && x.Value == "Bhbk.WebApi.Identity(Admins)").Any())
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class AuthorizeAdminsRequirement : IAuthorizationRequirement
    {

    }
}
