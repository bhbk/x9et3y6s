using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Internal.Infrastructure
{
    public class AuthorizeSystems : AuthorizationHandler<AuthorizeSystemsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeSystemsRequirement requirement)
        {
            if (context.User.HasClaim(x => x.Type == ClaimTypes.Role
                 && x.Value == "(Built-In) Systems"))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class AuthorizeSystemsRequirement : IAuthorizationRequirement
    {
        public AuthorizeSystemsRequirement() { }
    }
}
