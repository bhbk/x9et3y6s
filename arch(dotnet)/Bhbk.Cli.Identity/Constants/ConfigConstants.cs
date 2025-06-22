using System.Collections.Generic;

namespace Bhbk.Cli.Identity.Constants
{
    public static class ConfigConstants
    {
        public const string EncryptedValuePrefix = "AES:";
        public const string EncryptionKeyEnvVar = "CONFIG_ENCRYPTION_KEY";

        /*
         * Paths to sensitive string values or arrays of strings.
         */
        public static readonly IReadOnlyList<string> SensitivePaths = new List<string>
        {
            "Databases:IdentityEntities_EF",
            "Databases:IdentityEntities_EF",
            "IdentityTenant:AllowedIssuerKeys",
            "IdentityCredential:AudienceSecret",
            "Jobs:EmailDequeue:SendgridApiKey",
            "Jobs:TextDequeue:TwilioSid",
            "Jobs:TextDequeue:TwilioToken",
            "SeedData:Issuer:IssuerKey",
            "SeedData:Login:LoginKey",
        };

        /*
         * Array paths where each element contains a sensitive field.
         * Format: "ArrayPath:FieldName" e.g. "SeedData:Audiences:Password"
         */
        public static readonly IReadOnlyList<string> SensitiveArrayFields = new List<string>
        {
            "SeedData:Audiences:Password",
            "SeedData:Users:Password",
        };
    }
}
