using Bhbk.Lib.Identity.Model;
using System;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Factory
{
    public class ModelFactory
    {
        internal static CustomIdentityDbContext _context;
        public Born Create;
        public BLToEF Devolve;
        public EFToBL Evolve;
        public Change Update;

        public ModelFactory(CustomIdentityDbContext context)
        {
            _context = context;

            Create = new Born();
            Devolve = new BLToEF();
            Evolve = new EFToBL();
            Update = new Change();
        }

        public class Born
        {
            public Born() { }

            public AudienceModel DoIt(AudienceCreate audience)
            {
                return new AudienceModel()
                {
                    Id = Guid.NewGuid(),
                    ClientId = audience.ClientId,
                    Name = audience.Name,
                    Description = audience.Description,
                    AudienceType = audience.AudienceType,
                    AudienceKey = audience.AudienceKey,
                    Enabled = audience.Enabled,
                    Created = DateTime.Now,
                    Immutable = false
                };
            }

            public ClientModel DoIt(ClientCreate client)
            {
                return new ClientModel
                {
                    Id = Guid.NewGuid(),
                    Name = client.Name,
                    Description = client.Description,
                    Enabled = client.Enabled,
                    Created = DateTime.Now,
                    Immutable = false
                };
            }

            public ProviderModel DoIt(ProviderCreate provider)
            {
                return new ProviderModel
                {
                    Id = Guid.NewGuid(),
                    Name = provider.Name,
                    Description = provider.Description,
                    Enabled = provider.Enabled,
                    Created = DateTime.Now,
                    Immutable = false
                };
            }

            public RoleModel DoIt(RoleCreate role)
            {
                return new RoleModel
                {
                    Id = Guid.NewGuid(),
                    AudienceId = role.AudienceId,
                    Name = role.Name,
                    Description = role.Description,
                    Enabled = role.Enabled,
                    Created = DateTime.Now,
                    Immutable = false
                };
            }

            public UserModel DoIt(UserCreate user)
            {
                return new UserModel
                {
                    Id = Guid.NewGuid(),
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Created = DateTime.Now,
                    LockoutEnabled = user.LockoutEnabled,
                    Immutable = false
                };
            }

            public UserRefreshTokenModel DoIt(UserRefreshTokenCreate token)
            {
                return new UserRefreshTokenModel
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

            public AppAudience DoIt(AudienceModel audience)
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

            public AppClient DoIt(ClientModel client)
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

            public AppProvider DoIt(ProviderModel provider)
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

            public AppRole DoIt(RoleModel role)
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

            public AppUser DoIt(UserModel user)
            {
                return new AppUser
                {
                    Id = user.Id,
                    UserName = user.Email,
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

            public AppUserClaim DoIt(Guid userId, Guid claimId, Claim claim)
            {
                return new AppUserClaim
                {
                    Id = claimId,
                    UserId = userId,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    ClaimValueType = claim.ValueType
                };
            }

            public AppUserRefreshToken DoIt(UserRefreshTokenModel token)
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

            public AudienceModel DoIt(AppAudience audience)
            {
                return new AudienceModel()
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

            public ClientModel DoIt(AppClient client)
            {
                return new ClientModel
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

            public ProviderModel DoIt(AppProvider provider)
            {
                return new ProviderModel
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

            public RoleModel DoIt(AppRole role)
            {
                return new RoleModel
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

            public UserModel DoIt(AppUser user)
            {
                return new UserModel
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

            public Claim DoIt(AppUserClaim claim)
            {
                return new Claim(claim.ClaimType,
                    claim.ClaimValue,
                    claim.ClaimValueType);
            }

            public UserRefreshTokenModel DoIt(AppUserRefreshToken token)
            {
                return new UserRefreshTokenModel
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

        public class Change
        {
            public Change() { }

            public AudienceModel DoIt(AudienceUpdate audience)
            {
                return new AudienceModel()
                {
                    Id = audience.Id,
                    ClientId = audience.ClientId,
                    Name = audience.Name,
                    Description = audience.Description,
                    AudienceType = audience.AudienceType,
                    AudienceKey = audience.AudienceKey,
                    Enabled = audience.Enabled,
                    Immutable = audience.Immutable
                };
            }

            public ClientModel DoIt(ClientUpdate client)
            {
                return new ClientModel
                {
                    Id = client.Id,
                    Name = client.Name,
                    Description = client.Description,
                    Enabled = client.Enabled,
                    Immutable = client.Immutable
                };
            }

            public ProviderModel DoIt(ProviderUpdate provider)
            {
                return new ProviderModel
                {
                    Id = provider.Id,
                    Name = provider.Name,
                    Description = provider.Description,
                    Enabled = provider.Enabled,
                    Immutable = provider.Immutable
                };
            }

            public RoleModel DoIt(RoleUpdate role)
            {
                return new RoleModel
                {
                    Id = role.Id,
                    AudienceId = role.AudienceId,
                    Name = role.Name,
                    Description = role.Description,
                    Enabled = role.Enabled,
                    Immutable = role.Immutable
                };
            }

            public UserModel DoIt(UserUpdate user)
            {
                var current = _context.AppUser.Where(x => x.Id == user.Id).Single();

                return new UserModel
                {
                    Id = user.Id,
                    Email = current.Email,
                    EmailConfirmed = current.EmailConfirmed,
                    PhoneNumber = current.PhoneNumber,
                    PhoneNumberConfirmed = current.PhoneNumberConfirmed,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Created = current.Created,
                    LastUpdated = current.LastUpdated,
                    LockoutEnabled = current.LockoutEnabled,
                    LockoutEndDateUtc = current.LockoutEndDateUtc,
                    LastLoginFailure = current.LastLoginFailure,
                    LastLoginSuccess = current.LastLoginSuccess,
                    AccessFailedCount = current.AccessFailedCount,
                    AccessSuccessCount = current.AccessSuccessCount,
                    PasswordConfirmed = current.PasswordConfirmed,
                    TwoFactorEnabled = current.TwoFactorEnabled,
                    Immutable = current.Immutable,
                };
            }
        }
    }
}
