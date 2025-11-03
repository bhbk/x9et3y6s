using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Primitives.Enums;

namespace Bhbk.Test.Identity.Integration.TestingTools
{
    public static class ScenarioMother
    {
        /*
         * issuer only - for tests that just need an issuer
         */
        public static ISeedContext CreateIssuerSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .Seed();
        }

        /*
         * issuer + audience - for audience-related tests
         */
        public static ISeedContext CreateAudienceSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .Seed();
        }

        /*
         * issuer + audience + role - for role-related tests
         */
        public static ISeedContext CreateRoleSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .WithRole(r => r
                    .WithDefaults()
                    .ForAudience(System.Guid.Empty))
                .Seed();
        }

        /*
         * issuer + login provider - for login provider tests
         */
        public static ISeedContext CreateLoginProviderSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithLoginProvider(lp => lp.WithDefaults())
                .Seed();
        }

        /*
         * issuer + claim - for claim-related tests
         */
        public static ISeedContext CreateClaimSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithClaim(c => c.WithDefaults())
                .Seed();
        }

        /*
         * issuer + user - for user-related tests
         */
        public static ISeedContext CreateUserSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithUser(u => u.WithDefaults())
                .Seed();
        }

        /*
         * full seed - issuer + audience + role + login provider + user
         */
        public static ISeedContext CreateFullSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults().WithStandardRoles())
                .WithLoginProvider(lp => lp.WithDefaults())
                .WithClaim(c => c.WithDefaults())
                .WithUser(u => u.WithDefaults())
                .Seed();
        }

        /*
         * credentials test - issuer + audience + user with password
         * for email/password/phone change tests
         */
        public static ISeedContext CreateCredentialsTestSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults().WithStandardRoles())
                .WithUser(u => u.WithDefaults().AsUser())
                .Seed();
        }

        /*
         * OAuth implicit flow - issuer + audience + user + URL + state token
         */
        public static ISeedContext CreateOAuthImplicitSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults().WithStandardRoles())
                .WithUrl(u => u.WithDefaults())
                .WithUser(u => u.WithDefaults().AsUser())
                .WithState(s => s.WithDefaults().WithStateType(ConsumerType.User))
                .Seed();
        }

        /*
         * OAuth authorization code flow - issuer + audience + user + URL + state token
         */
        public static ISeedContext CreateOAuthAuthCodeSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults().WithStandardRoles())
                .WithUrl(u => u.WithDefaults())
                .WithUser(u => u.WithDefaults().AsUser())
                .WithState(s => s.WithDefaults().WithStateType(ConsumerType.User))
                .Seed();
        }

        /*
         * OAuth resource owner password grant - issuer + audience + user with password
         */
        public static ISeedContext CreateOAuthResourceOwnerSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults().WithStandardRoles())
                .WithUser(u => u.WithDefaults().AsUser())
                .Seed();
        }

        /*
         * OAuth device code flow - issuer + audience + user + device state token
         */
        public static ISeedContext CreateOAuthDeviceSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults().WithStandardRoles())
                .WithUser(u => u.WithDefaults().AsUser())
                .WithState(s => s.WithDefaults().WithStateType(ConsumerType.Device))
                .Seed();
        }

        /*
         * session/refresh token tests - issuer + audience + user + refresh token
         */
        public static ISeedContext CreateSessionTestSeed(IUnitOfWork uow)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults().WithStandardRoles())
                .WithUser(u => u.WithDefaults().AsUser())
                .WithRefresh(r => r.WithDefaults())
                .Seed();
        }

        /*
         * End2End tests - uses entities from testsettings.json
         */
        public static ISeedContext CreateFromTestSettings(IUnitOfWork uow, TestDataSettings testData)
        {
            return new SeedContext(uow)
                .WithIssuer(i => i.FromTestSettings(testData.Issuer))
                .WithAudience(a => a.FromTestSettings(testData.Audience))
                .WithRole(r => r.FromTestSettings(testData.Role))
                .WithLoginProvider(lp => lp.FromTestSettings(testData.LoginProvider))
                .WithUser(u => u.FromTestSettings(testData.User).WithRole(testData.Role.Name))
                .Seed();
        }
    }
}
