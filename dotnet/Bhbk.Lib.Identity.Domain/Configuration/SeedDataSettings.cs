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
    }

    public class IdentityProviderSettings
    {
        public string BuiltInLoginProviderName { get; set; }
        public string BuiltInIssuerName { get; set; }
        public string TestLoginProviderNamePrefix { get; set; }
        public string TestIssuerName { get; set; }
    }

    public class SeedDataSettings
    {
        public SeedIssuerSettings Issuer { get; set; }
        public SeedLoginProviderSettings LoginProvider { get; set; }
        public List<SeedLLMProviderSettings> LLMProviders { get; set; }
        public List<SeedJobSettings> Jobs { get; set; }
        public List<SeedAudienceSettings> Audiences { get; set; }
        public List<SeedRoleSettings> Roles { get; set; }
        public List<SeedEntitlementTypeSettings> EntitlementTypes { get; set; }
        public List<SeedEntitlementScopeSettings> EntitlementScopes { get; set; }
        public List<SeedUserEntitlementSettings> UserEntitlements { get; set; }
        public List<SeedAudienceEntitlementSettings> AudienceEntitlements { get; set; }
        public List<SeedUserSettings> Users { get; set; }
    }

    public class SeedJobSettings
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public List<SeedJobSettingItem> Settings { get; set; }
    }

    public class SeedJobSettingItem
    {
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public bool IsSecret { get; set; }
    }

    public class SeedLLMProviderSettings
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public int FailoverOrder { get; set; }
        public List<SeedLLMProviderSettingItem> Settings { get; set; }
    }

    public class SeedLLMProviderSettingItem
    {
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public bool IsSecret { get; set; }
    }

    public class SeedIssuerSettings
    {
        public string Name { get; set; }
        public string IssuerKey { get; set; }
    }

    public class SeedLoginProviderSettings
    {
        public string Name { get; set; }
        public string ProviderKey { get; set; }
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

    public class SeedEntitlementTypeSettings
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
    }

    public class SeedEntitlementScopeSettings
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
    }

    public class SeedUserEntitlementSettings
    {
        public string UserName { get; set; }
        public string EntitlementTypeName { get; set; }
        public string EntitlementScopeName { get; set; }
        public string IssuerName { get; set; }
        public string AudienceName { get; set; }
    }

    public class SeedAudienceEntitlementSettings
    {
        public string AudienceName { get; set; }
        public string EntitlementTypeName { get; set; }
        public string EntitlementScopeName { get; set; }
        public string IssuerName { get; set; }
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
        public TestLoginProviderSettings LoginProvider { get; set; }
        public TestAudienceSettings Audience { get; set; }
        public TestClaimSettings Claim { get; set; }
        public TestEmailSettings Email { get; set; }
        public TestTextSettings Text { get; set; }
        public TestQuoteSettings Quote { get; set; }
        public TestRoleSettings Role { get; set; }
        public TestUserSettings User { get; set; }
        public TestUrlSettings Url { get; set; }
        public SeedEntitySettings Seed { get; set; }
    }

    public class SeedEntitySettings
    {
        public string IssuerName { get; set; }
        public string LoginProviderName { get; set; }
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

    public class TestLoginProviderSettings
    {
        public string Name { get; set; }
        public string ProviderKey { get; set; }
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

    public class TestQuoteSettings
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
