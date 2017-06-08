using Bhbk.Lib.Identity.Model;
using Bhbk.Lib.Identity.Store;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class CustomModelFactory
    {
        private IUnitOfWork _uow;

        public CustomModelFactory(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public AudienceModel.Return.Audience Create(AppAudience audience)
        {
            _uow.AudienceRepository.Attach(audience);
            _uow.AudienceRepository.LoadCollection(audience, "Roles");
             
            return new AudienceModel.Return.Audience()
            {
                Id = audience.Id,
                ClientId = audience.ClientId,
                Name = audience.Name,
                Description = audience.Description,
                AudienceKey = audience.AudienceKey,
                AudienceType = audience.AudienceType,
                Enabled = audience.Enabled,
                Created = audience.Created,
                Immutable = audience.Immutable,
                Roles = audience.Roles.Select(x => x.Name).ToList()
            };
        }

        public ClientModel.Return.Client Create(AppClient client)
        {
            _uow.ClientRepository.Attach(client);
            _uow.ClientRepository.LoadCollection(client, "Audiences");

            return new ClientModel.Return.Client
            {
                Id = client.Id,
                Name = client.Name,
                Enabled = client.Enabled,
                Created = client.Created,
                Immutable = client.Immutable,
                Audiences = client.Audiences.Select(x => x.Name).ToList()
            };
        }

        public ProviderModel.Return.Provider Create(AppProvider provider)
        {
            return new ProviderModel.Return.Provider
            {
                Id = provider.Id,
                Name = provider.Name,
                Enabled = provider.Enabled,
                Created = provider.Created,
                Immutable = provider.Immutable
            };
        }

        public RoleModel.Return.Role Create(AppRole role)
        {
            return new RoleModel.Return.Role
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Created = role.Created,
                LastUpdated = role.LastUpdated,
                Enabled = role.Enabled,
                Immutable = role.Immutable,
                AudienceId = role.AudienceId
            };
        }

        public UserModel.Return.User Create(AppUser user)
        {
            _uow.UserRepository.Attach(user);
            _uow.UserRepository.LoadCollection(user, "Claims");

            return new UserModel.Return.User
            {
                Id = user.Id,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Created = user.Created,
                LastUpdated = user.LastUpdated.HasValue ? user.LastUpdated.Value : user.LastUpdated,
                AccessFailedCount = user.AccessFailedCount,
                AccessSuccessCount = user.AccessSuccessCount,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEndDateUtc = user.LockoutEndDateUtc.HasValue ? user.LockoutEndDateUtc.Value.ToLocalTime() : user.LockoutEndDateUtc,
                TwoFactorEnabled = user.TwoFactorEnabled,
                Immutable = user.Immutable,
                Claims = user.Claims.ToList(),
                Roles = user.Roles.ToList()
            };
        }

        public UserClaimModel.Return.Claim Create(AppUserClaim claim)
        {
            return new UserClaimModel.Return.Claim
            {
                Id = claim.Id,
                UserId = claim.UserId,
                ClaimType = claim.ClaimType,
                ClaimValue = claim.ClaimType,
                Created = claim.Created,
                Immutable = claim.Immutable
            };
        }
    }

    public class AudienceModel
    {
        public class Return
        {
            public class Audience
            {
                public Guid Id { get; set; }
                public Guid ClientId { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public string AudienceKey { get; set; }
                public string AudienceType { get; set; }
                public bool Enabled { get; set; }
                public DateTime Created { get; set; }
                public Nullable<DateTime> LastUpdated { get; set; }
                public bool Immutable { get; set; }
                public IList<string> Roles { get; set; }
            }
        }

        public class Binding
        {
            public class Create
            {
                public Guid ClientId { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public string AudienceType { get; set; }
                public bool Enabled { get; set; }
                public bool Immutable { get; set; }
            }

            public class Update
            {
                public Guid Id { get; set; }
                public Guid ClientId { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public string AudienceType { get; set; }
                public bool Enabled { get; set; }
                public bool Immutable { get; set; }
            }
        }
    }

    public class ConfigModel
    {
        public bool Debug { get; set; }
        public UInt16 DefaultPassMinLength { get; set; }
        public Double DefaultAuthorizationCodeExpire { get; set; }
        public Double DefaultTokenExpire { get; set; }
        public Double DefaultRefreshTokenExpire { get; set; }
        public string IdentityAdminBaseUrl { get; set; }
        public string IdentityMeBaseUrl { get; set; }
        public string IdentityStsBaseUrl { get; set; }
    }

    public class ClientModel
    {
        public class Return
        {
            public class Client
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public bool Enabled { get; set; }
                public DateTime Created { get; set; }
                public Nullable<DateTime> LastUpdated { get; set; }
                public bool Immutable { get; set; }
                public IList<string> Audiences { get; set; }
            }
        }

        public class Binding
        {
            public class Create
            {
                public string Name { get; set; }
                public string Description { get; set; }
                public bool Enabled { get; set; }
                public bool Immutable { get; set; }
            }

            public class Update
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public bool Immutable { get; set; }
                public bool Enabled { get; set; }
            }
        }
    }

    public class ProviderModel
    {
        public class Return
        {
            public class Provider
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public bool Enabled { get; set; }
                public DateTime Created { get; set; }
                public Nullable<DateTime> LastUpdated { get; set; }
                public bool Immutable { get; set; }
                public IList<string> Users { get; set; }
            }
        }

        public class Binding
        {
            public class Create
            {
                public string Name { get; set; }
                public string Description { get; set; }
                public bool Enabled { get; set; }
                public bool Immutable { get; set; }
            }

            public class Update
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public bool Immutable { get; set; }
                public bool Enabled { get; set; }
            }
        }
    }

    public class RoleModel
    {
        public class Return
        {
            public class Role
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public DateTime Created { get; set; }
                public Nullable<DateTime> LastUpdated { get; set; }
                public bool Enabled { get; set; }
                public bool Immutable { get; set; }
                public Guid AudienceId { get; set; }
            }
        }

        public class Binding
        {
            public class Create
            {
                public string Name { get; set; }
                public string Description { get; set; }
                public bool Enabled { get; set; }
                public bool Immutable { get; set; }
                public Guid AudienceId { get; set; }
            }

            public class Update
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public bool Enabled { get; set; }
                public bool Immutable { get; set; }
                public Guid AudienceId { get; set; }
            }
        }
    }

    public class UserModel
    {
        public class Binding
        {
            public class Create
            {
                public string Email { get; set; }
                public bool EmailConfirmed { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public bool LockoutEnabled { get; set; }
                public bool TwoFactorEnabled { get; set; }
                public bool Immutable { get; set; }
            }

            public class Update
            {
                public Guid Id { get; set; }
                public string Email { get; set; }
                public bool EmailConfirmed { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public bool LockoutEnabled { get; set; }
                public Nullable<DateTime> LockoutEndDateUtc { get; set; }
                public bool TwoFactorEnabled { get; set; }
                public bool Immutable { get; set; }
            }

            public class ChangePassword
            {
                public string CurrentPassword { get; set; }
                public string NewPassword { get; set; }
                public string NewPasswordConfirm { get; set; }
            }

            public class SetPassword
            {
                public string NewPassword { get; set; }
                public string NewPasswordConfirm { get; set; }
            }
        }

        public class Return
        {
            public class User
            {
                public Guid Id { get; set; }
                public string Email { get; set; }
                public bool EmailConfirmed { get; set; }
                public string PhoneNumber { get; set; }
                public Nullable<bool> PhoneNumberConfirmed { get; set; }
                public DateTime Created { get; set; }
                public Nullable<DateTime> LastUpdated { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public bool LockoutEnabled { get; set; }
                public Nullable<DateTime> LockoutEndDateUtc { get; set; }
                public Nullable<DateTime> LastLoginFailure { get; set; }
                public Nullable<DateTime> LastLoginSuccess { get; set; }
                public int AccessFailedCount { get; set; }
                public int AccessSuccessCount { get; set; }
                public bool TwoFactorEnabled { get; set; }
                public bool Immutable { get; set; }
                public IList<AppUserClaim> Claims { get; set; }
                public IList<AppUserRole> Roles { get; set; }
            }
        }
    }

    public class UserClaimModel
    {
        public class Return
        {
            public class Claim
            {
                public Guid Id { get; set; }
                public Guid UserId { get; set; }
                public string ClaimType { get; set; }
                public string ClaimValue { get; set; }
                public string ClaimValueType { get; set; }
                public string Issuer { get; set; }
                public string OriginalIssuer { get; set; }
                public string Subject { get; set; }
                public IDictionary<string, string> Properties { get; set; }
                public DateTime Created { get; set; }
                public Nullable<DateTime> LastUpdated { get; set; }
                public bool Immutable { get; set; }
            }
        }

        public class Binding
        {
            public class Create
            {
                public Guid UserId { get; set; }
                public string ClaimType { get; set; }
                public string ClaimValue { get; set; }
                public string ClaimValueType { get; set; }
                public string Issuer { get; set; }
                public string OriginalIssuer { get; set; }
                public string Subject { get; set; }
                public IDictionary<string, string> Properties { get; set; }
                public bool Immutable { get; set; }
            }

            public class Update
            {
                public Guid Id { get; set; }
                public Guid UserId { get; set; }
                public string ClaimType { get; set; }
                public string ClaimValue { get; set; }
                public string ClaimValueType { get; set; }
                public string Issuer { get; set; }
                public string OriginalIssuer { get; set; }
                public string Subject { get; set; }
                public IDictionary<string, string> Properties { get; set; }
                public DateTime Created { get; set; }
                public Nullable<DateTime> LastUpdated { get; set; }
                public bool Immutable { get; set; }
            }
        }
    }
}
