using Bhbk.Lib.Identity.Model;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Infrastructure
{
    public class ModelFactory
    {
        private CustomIdentityDbContext _context;
        public Born Create;
        public BLToEF Devolve;
        public EFToBL Evolve;
        public Die Destroy;

        public ModelFactory(CustomIdentityDbContext context)
        {
            _context = context;

            Create = new Born();
            Devolve = new BLToEF();
            Evolve = new EFToBL();
            Destroy = new Die();
        }

        public class Born
        {
            public Born() { }

            public AudienceModel.Model DoIt(AudienceModel.Create audience)
            {
                return new AudienceModel.Model()
                {
                    Id = Guid.NewGuid(),
                    ClientId = audience.ClientId,
                    Name = audience.Name,
                    Description = audience.Description,
                    AudienceType = audience.AudienceType,
                    AudienceKey = audience.AudienceKey,
                    Enabled = audience.Enabled,
                    Created = DateTime.Now,
                    Immutable = audience.Immutable
                };
            }

            public ClientModel.Model DoIt(ClientModel.Create client)
            {
                return new ClientModel.Model
                {
                    Id = Guid.NewGuid(),
                    Name = client.Name,
                    Description = client.Description,
                    Enabled = client.Enabled,
                    Created = DateTime.Now,
                    Immutable = client.Immutable
                };
            }

            public ProviderModel.Model DoIt(ProviderModel.Create provider)
            {
                return new ProviderModel.Model
                {
                    Id = Guid.NewGuid(),
                    Name = provider.Name,
                    Description = provider.Description,
                    Enabled = provider.Enabled,
                    Created = DateTime.Now,
                    Immutable = provider.Immutable
                };
            }

            public RoleModel.Model DoIt(RoleModel.Create role)
            {
                return new RoleModel.Model
                {
                    Id = Guid.NewGuid(),
                    AudienceId = role.AudienceId,
                    Name = role.Name,
                    Description = role.Description,
                    Enabled = role.Enabled,
                    Created = DateTime.Now,
                    Immutable = role.Immutable
                };
            }

            public UserModel.Model DoIt(UserModel.Create user)
            {
                return new UserModel.Model
                {
                    Id = Guid.NewGuid(),
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Created = DateTime.Now,
                    LockoutEnabled = user.LockoutEnabled,
                    Immutable = user.Immutable
                };
            }

            public UserClaimModel.Model DoIt(UserClaimModel.Create claim)
            {
                return new UserClaimModel.Model
                {
                    Id = Guid.NewGuid(),
                    UserId = claim.UserId,
                    ClaimType = claim.ClaimType,
                    ClaimValue = claim.ClaimType,
                    Created = DateTime.Now,
                    Immutable = claim.Immutable
                };
            }

            public UserRefreshTokenModel.Model DoIt(UserRefreshTokenModel.Create token)
            {
                return new UserRefreshTokenModel.Model
                {
                    Id = Guid.NewGuid(),
                    ClientId = token.ClientId,
                    AudienceId = token.AudienceId,
                    UserId = token.UserId,
                    ProtectedTicket = token.ProtectedTicket,
                    IssuedUtc = token.IssuedUtc,
                    ExpiresUtc = token.ExpiresUtc
                };
            }
        }

        public class BLToEF
        {
            public BLToEF() { }

            public AppAudience DoIt(AudienceModel.Model audience)
            {
                return new AppAudience
                {
                    Id = audience.Id,
                    ClientId = audience.ClientId,
                    Name = audience.Name,
                    Description = audience.Description,
                    AudienceType = audience.AudienceType,
                    AudienceKey = audience.AudienceKey,
                    Enabled = audience.Enabled,
                    Created = audience.Created,
                    LastUpdated = audience.LastUpdated,
                    Immutable = audience.Immutable
                };
            }

            public AppClient DoIt(ClientModel.Model client)
            {
                return new AppClient
                {
                    Id = client.Id,
                    Name = client.Name,
                    Description = client.Description,
                    Enabled = client.Enabled,
                    Created = client.Created,
                    LastUpdated = client.LastUpdated,
                    Immutable = client.Immutable
                };
            }

            public AppProvider DoIt(ProviderModel.Model provider)
            {
                return new AppProvider
                {
                    Id = provider.Id,
                    Name = provider.Name,
                    Description = provider.Description,
                    Enabled = provider.Enabled,
                    Created = provider.Created,
                    LastUpdated = provider.LastUpdated,
                    Immutable = provider.Immutable
                };
            }

            public AppRole DoIt(RoleModel.Model role)
            {
                return new AppRole
                {
                    Id = role.Id,
                    AudienceId = role.AudienceId,
                    Name = role.Name,
                    Description = role.Description,
                    Enabled = role.Enabled,
                    Created = role.Created,
                    LastUpdated = role.LastUpdated,
                    Immutable = role.Immutable
                };
            }

            public AppUser DoIt(UserModel.Model user)
            {
                return new AppUser
                {
                    Id = user.Id,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Created = user.Created,
                    LastUpdated = user.LastUpdated,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEndDateUtc = user.LockoutEndDateUtc,
                    LastLoginFailure = user.LastLoginFailure,
                    LastLoginSuccess = user.LastLoginSuccess,
                    AccessFailedCount = user.AccessFailedCount,
                    AccessSuccessCount = user.AccessSuccessCount,
                    PasswordConfirmed = user.PasswordConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    Immutable = user.Immutable,
                };
            }

            public AppUserClaim DoIt(UserClaimModel.Model claim)
            {
                return new AppUserClaim
                {
                    Id = claim.Id,
                    UserId = claim.UserId,
                    ClaimType = claim.ClaimType,
                    ClaimValue = claim.ClaimType,
                    ClaimValueType = claim.ClaimValueType,
                    Issuer = claim.Issuer,
                    OriginalIssuer = claim.OriginalIssuer,
                    Subject = claim.Subject,
                    Created = claim.Created,
                    LastUpdated = claim.LastUpdated,
                    Immutable = claim.Immutable
                };
            }

            public AppUserRefreshToken DoIt(UserRefreshTokenModel.Model token)
            {
                return new AppUserRefreshToken
                {
                    Id = token.Id,
                    ClientId = token.ClientId,
                    AudienceId = token.AudienceId,
                    UserId = token.UserId,
                    ProtectedTicket = token.ProtectedTicket,
                    IssuedUtc = token.IssuedUtc,
                    ExpiresUtc = token.ExpiresUtc
                };
            }
        }

        public class EFToBL
        {
            public EFToBL() { }

            public AudienceModel.Model DoIt(AppAudience audience)
            {
                return new AudienceModel.Model()
                {
                    Id = audience.Id,
                    ClientId = audience.ClientId,
                    Name = audience.Name,
                    Description = audience.Description,
                    AudienceType = audience.AudienceType,
                    AudienceKey = audience.AudienceKey,
                    Enabled = audience.Enabled,
                    Created = audience.Created,
                    LastUpdated = audience.LastUpdated,
                    Immutable = audience.Immutable,
                };
            }

            public ClientModel.Model DoIt(AppClient client)
            {
                return new ClientModel.Model
                {
                    Id = client.Id,
                    Name = client.Name,
                    Description = client.Description,
                    Enabled = client.Enabled,
                    Created = client.Created,
                    LastUpdated = client.LastUpdated,
                    Immutable = client.Immutable
                };
            }

            public ProviderModel.Model DoIt(AppProvider provider)
            {
                return new ProviderModel.Model
                {
                    Id = provider.Id,
                    Name = provider.Name,
                    Description = provider.Description,
                    Enabled = provider.Enabled,
                    Created = provider.Created,
                    LastUpdated = provider.LastUpdated,
                    Immutable = provider.Immutable
                };
            }

            public RoleModel.Model DoIt(AppRole role)
            {
                return new RoleModel.Model
                {
                    Id = role.Id,
                    AudienceId = role.AudienceId,
                    Name = role.Name,
                    Description = role.Description,
                    Enabled = role.Enabled,
                    Created = role.Created,
                    LastUpdated = role.LastUpdated,
                    Immutable = role.Immutable
                };
            }

            public UserModel.Model DoIt(AppUser user)
            {
                return new UserModel.Model
                {
                    Id = user.Id,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Created = user.Created,
                    LastUpdated = user.LastUpdated,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEndDateUtc = user.LockoutEndDateUtc,
                    LastLoginFailure = user.LastLoginFailure,
                    LastLoginSuccess = user.LastLoginSuccess,
                    AccessFailedCount = user.AccessFailedCount,
                    AccessSuccessCount = user.AccessSuccessCount,
                    PasswordConfirmed = user.PasswordConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    Immutable = user.Immutable,
                };
            }

            public UserClaimModel.Model DoIt(AppUserClaim claim)
            {
                return new UserClaimModel.Model
                {
                    Id = claim.Id,
                    UserId = claim.UserId,
                    ClaimType = claim.ClaimType,
                    ClaimValue = claim.ClaimType,
                    ClaimValueType = claim.ClaimValueType,
                    Issuer = claim.Issuer,
                    OriginalIssuer = claim.OriginalIssuer,
                    Subject = claim.Subject,
                    Created = claim.Created,
                    LastUpdated = claim.LastUpdated,
                    Immutable = claim.Immutable
                };
            }

            public UserRefreshTokenModel.Model DoIt(AppUserRefreshToken token)
            {
                return new UserRefreshTokenModel.Model
                {
                    Id = token.Id,
                    ClientId = token.ClientId,
                    AudienceId = token.AudienceId,
                    UserId = token.UserId,
                    ProtectedTicket = token.ProtectedTicket,
                    IssuedUtc = token.IssuedUtc,
                    ExpiresUtc = token.ExpiresUtc
                };
            }
        }

        public class Die
        {
            public Die() { }
        }
    }

    public class AudienceModel
    {
        public class Create
        {
            public Guid ClientId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string AudienceType { get; set; }
            public string AudienceKey { get; set; }
            public bool Enabled { get; set; }
            public bool Immutable { get; set; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public Guid ClientId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string AudienceType { get; set; }
            public string AudienceKey { get; set; }
            public bool Enabled { get; set; }
            public DateTime Created { get; set; }
            public Nullable<DateTime> LastUpdated { get; set; }
            public bool Immutable { get; set; }
            //public IList<RoleModel.Model> Roles { get; set; }
        }

        public class Update
        {
            public Guid Id { get; set; }
            public Guid ClientId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string AudienceType { get; set; }
            public string AudienceKey { get; set; }
            public bool Enabled { get; set; }
            public bool Immutable { get; set; }
        }
    }

    public class ClientModel
    {
        public class Create
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; }
            public bool Immutable { get; set; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; }
            public DateTime Created { get; set; }
            public Nullable<DateTime> LastUpdated { get; set; }
            public bool Immutable { get; set; }
            public IList<AudienceModel.Model> Audiences { get; set; }
        }

        public class Update
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; }
            public bool Immutable { get; set; }
        }
    }

    public class ProviderModel
    {
        public class Create
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; }
            public bool Immutable { get; set; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; }
            public DateTime Created { get; set; }
            public Nullable<DateTime> LastUpdated { get; set; }
            public bool Immutable { get; set; }
            public IList<UserModel.Model> Users { get; set; }
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

    public class RoleModel
    {
        public class Create
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; }
            public bool Immutable { get; set; }
            public Guid AudienceId { get; set; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public Guid AudienceId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime Created { get; set; }
            public Nullable<DateTime> LastUpdated { get; set; }
            public bool Enabled { get; set; }
            public bool Immutable { get; set; }
        }

        public class Update
        {
            public Guid AudienceId { get; set; }
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; }
            public bool Immutable { get; set; }
        }
    }

    public class ConfigModel
    {
        public bool Debug { get; set; }
        public Double DefaultAccessTokenLife { get; set; }
        public UInt16 DefaultAuhthorizationCodeLife { get; set; }
        public UInt16 DefaultAuhthorizationCodeLength { get; set; }
        public UInt16 DefaultPasswordLength { get; set; }
        public UInt16 DefaultFailedAccessAttempts { get; set; }
        public Double DefaultRefreshTokenLife { get; set; }
        public string IdentityAdminBaseUrl { get; set; }
        public string IdentityMeBaseUrl { get; set; }
        public string IdentityStsBaseUrl { get; set; }
        public bool UnitTestAccessToken { get; set; }
        public DateTime UnitTestAccessTokenFakeUtcNow { get; set; }
        public bool UnitTestRefreshToken { get; set; }
        public DateTime UnitTestRefreshTokenFakeUtcNow { get; set; }
    }

    public class UserModel
    {
        public class AddPassword
        {
            public Guid Id { get; set; }
            public string NewPassword { get; set; }
            public string NewPasswordConfirm { get; set; }
        }

        public class AddPhoneNumber
        {
            public Guid Id { get; set; }
            public string NewPhoneNumber { get; set; }
            public string NewPhoneNumberConfirm { get; set; }
        }

        public class ChangePassword
        {
            public Guid Id { get; set; }
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string NewPasswordConfirm { get; set; }
        }

        public class ChangePhone
        {
            public Guid Id { get; set; }
            public string CurrentPhoneNumber { get; set; }
            public string NewPhoneNumber { get; set; }
            public string NewPhoneNumberConfirm { get; set; }
        }

        public class ChangeEmail
        {
            public Guid Id { get; set; }
            public string CurrentEmail { get; set; }
            public string NewEmail { get; set; }
            public string NewEmailConfirm { get; set; }
        }

        public class Create
        {
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime Created { get; set; }
            public bool LockoutEnabled { get; set; }
            public bool Immutable { get; set; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public bool EmailConfirmed { get; set; }
            public string PhoneNumber { get; set; }
            public Nullable<bool> PhoneNumberConfirmed { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime Created { get; set; }
            public Nullable<DateTime> LastUpdated { get; set; }
            public bool LockoutEnabled { get; set; }
            public Nullable<DateTime> LockoutEndDateUtc { get; set; }
            public Nullable<DateTime> LastLoginFailure { get; set; }
            public Nullable<DateTime> LastLoginSuccess { get; set; }
            public int AccessFailedCount { get; set; }
            public int AccessSuccessCount { get; set; }
            public bool PasswordConfirmed { get; set; }
            public bool TwoFactorEnabled { get; set; }
            public bool Immutable { get; set; }
            public IList<AppUserClaim> Claims { get; set; }
            public IList<AppUserRole> Roles { get; set; }
        }

        public class Update
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public bool LockoutEnabled { get; set; }
            public Nullable<DateTime> LockoutEndDateUtc { get; set; }
            public bool TwoFactorEnabled { get; set; }
            public bool Immutable { get; set; }
        }
    }

    public class UserClaimModel
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

        public class Model
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

    public class UserRefreshTokenModel
    {
        public class Create
        {
            public Guid ClientId { get; set; }
            public Guid AudienceId { get; set; }
            public Guid UserId { get; set; }
            public string ProtectedTicket { get; set; }
            public DateTime IssuedUtc { get; set; }
            public DateTime ExpiresUtc { get; set; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public Guid ClientId { get; set; }
            public Guid AudienceId { get; set; }
            public Guid UserId { get; set; }
            public string ProtectedTicket { get; set; }
            public DateTime IssuedUtc { get; set; }
            public DateTime ExpiresUtc { get; set; }
        }
    }
}
