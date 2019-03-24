using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public class AuthorizeUsers : AuthorizationHandler<AuthorizeUsersRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeUsersRequirement requirement)
        {
            if(context.User.Claims.Where(x => x.Type == ClaimTypes.Role
                && x.Value == "Bhbk.WebApi.Identity(Users)").Any())
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class AuthorizeUsersRequirement : IAuthorizationRequirement
    {

    }
}
