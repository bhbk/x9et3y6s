using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Domain.Authorize
{
    public class IdentityEntitlementAuthorize : AuthorizationHandler<IdentityEntitlementRequirement>
    {
        private static readonly string[] EntitlementHierarchy = { "Admin", "User", "Viewer" };

        private readonly IServiceProvider _serviceProvider;

        public IdentityEntitlementAuthorize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IdentityEntitlementRequirement requirement)
        {
            var systemClaim = context.User.FindFirst(ClaimTypes.System);
            var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (idClaim == null || !Guid.TryParse(idClaim.Value, out var principalId))
                return Task.CompletedTask;

            using var scope = _serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            /* dual-path: query correct assignment table based on token type */
            var entitlementTypeIds = systemClaim?.Value == AudienceType.server.ToString()
                ? uow.AudienceEntitlements.Get(x => x.AudienceId == principalId && x.IsEnabled)
                    .Select(x => x.EntitlementTypeId).Distinct().ToList()
                : uow.UserEntitlements.Get(x => x.UserId == principalId && x.IsEnabled)
                    .Select(x => x.EntitlementTypeId).Distinct().ToList();

            if (entitlementTypeIds.Count == 0)
                return Task.CompletedTask;

            var requiredLevel = Array.IndexOf(EntitlementHierarchy, requirement.MinimumEntitlementType);

            if (requiredLevel < 0)
                return Task.CompletedTask;

            var entitlementTypes = uow.EntitlementTypes.Get(x => entitlementTypeIds.Contains(x.Id) && x.IsEnabled)
                .ToList();

            foreach (var entType in entitlementTypes)
            {
                var typeLevel = Array.IndexOf(EntitlementHierarchy, entType.Name);

                if (typeLevel >= 0 && typeLevel <= requiredLevel)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }
    }

    public class IdentityEntitlementRequirement : IAuthorizationRequirement
    {
        public string MinimumEntitlementType { get; }

        public IdentityEntitlementRequirement(string minimumEntitlementType)
        {
            MinimumEntitlementType = minimumEntitlementType;
        }
    }
}
