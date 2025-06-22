using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Domain.Configuration
{
    public class AuthorizationSettings
    {
        public RoleSettings Roles { get; set; }
    }

    public class RoleSettings
    {
        public string AlertAdmins { get; set; }
        public string AlertUsers { get; set; }
        public string AlertViewers { get; set; }
        public string IdentityAdmins { get; set; }
        public string IdentityUsers { get; set; }
        public string IdentityViewers { get; set; }
    }

    public class IdentityProviderSettings
    {
        public string BuiltInLoginName { get; set; }
        public string BuiltInIssuerName { get; set; }
        public string TestLoginNamePrefix { get; set; }
        public string TestIssuerName { get; set; }
    }

    public class SeedDataSettings
    {
        public SeedIssuerSettings Issuer { get; set; }
        public SeedLoginSettings Login { get; set; }
        public List<SeedAudienceSettings> Audiences { get; set; }
        public List<SeedRoleSettings> Roles { get; set; }
        public List<SeedUserSettings> Users { get; set; }
    }

    public class SeedIssuerSettings
    {
        public string Name { get; set; }
        public string IssuerKey { get; set; }
    }

    public class SeedLoginSettings
    {
        public string Name { get; set; }
        public string LoginKey { get; set; }
    }

    public class SeedAudienceSettings
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class SeedRoleSettings
    {
        public string Name { get; set; }
        public string AudienceName { get; set; }
    }

    public class SeedUserSettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; }
    }

    public class TestDataSettings
    {
        public TestIssuerSettings Issuer { get; set; }
        public TestLoginSettings Login { get; set; }
        public TestAudienceSettings Audience { get; set; }
        public TestClaimSettings Claim { get; set; }
        public TestEmailSettings Email { get; set; }
        public TestTextSettings Text { get; set; }
        public TestMOTDSettings MOTD { get; set; }
        public TestRoleSettings Role { get; set; }
        public TestUserSettings User { get; set; }
        public TestUrlSettings Url { get; set; }
        public SeedEntitySettings Seed { get; set; }
    }

    public class SeedEntitySettings
    {
        public string IssuerName { get; set; }
        public string LoginName { get; set; }
        public string AudienceNameAlert { get; set; }
        public string AudienceNameIdentity { get; set; }
        public string RoleForAdminsAlert { get; set; }
        public string RoleForUsersAlert { get; set; }
        public string RoleForViewersAlert { get; set; }
        public string RoleForAdminsIdentity { get; set; }
        public string RoleForUsersIdentity { get; set; }
        public string RoleForViewersIdentity { get; set; }
        public string UserNameAdmin { get; set; }
        public string UserNameNormal { get; set; }
        public string UserNameViewer { get; set; }
    }

    public class TestIssuerSettings
    {
        public string Name { get; set; }
        public string IssuerKey { get; set; }
    }

    public class TestLoginSettings
    {
        public string Name { get; set; }
        public string LoginKey { get; set; }
    }

    public class TestAudienceSettings
    {
        public string Name { get; set; }
        public string PasswordCurrent { get; set; }
        public string PasswordNew { get; set; }
    }

    public class TestClaimSettings
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public string ValueType { get; set; }
    }

    public class TestEmailSettings
    {
        public string Content { get; set; }
        public string Subject { get; set; }
    }

    public class TestTextSettings
    {
        public string Content { get; set; }
    }

    public class TestMOTDSettings
    {
        public string Author { get; set; }
    }

    public class TestRoleSettings
    {
        public string Name { get; set; }
    }

    public class TestUserSettings
    {
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordCurrent { get; set; }
        public string PasswordNew { get; set; }
    }

    public class TestUrlSettings
    {
        public string Name { get; set; }
        public string Link { get; set; }
    }
}
