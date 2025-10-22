namespace Bhbk.Lib.Identity.Primitives.Constants
{
    public static class PolicyConstants
    {
        /* OAuth2 grant type policies */
        public const string OAuth2ROPGrants = "OAuth2ResourceOwnerPassword";
        public const string OAuth2CCGrants = "OAuth2ClientCredential";

        /* Alert service role-based policies */
        public const string AlertAdminPolicy = "AlertAdmin";
        public const string AlertUserPolicy = "AlertUser";
        public const string AlertViewerPolicy = "AlertViewer";

        /* Entitlement-based policies (database-driven RBAC) */
        public const string EntitlementAdminPolicy = "EntitlementAdmin";
        public const string EntitlementUserPolicy = "EntitlementUser";
        public const string EntitlementViewerPolicy = "EntitlementViewer";
    }
}
