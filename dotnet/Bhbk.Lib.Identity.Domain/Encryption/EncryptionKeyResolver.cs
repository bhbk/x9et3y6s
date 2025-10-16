using System;
using System.IO;

namespace Bhbk.Lib.Identity.Domain.Encryption
{
    public static class EncryptionKeyResolver
    {
        public const string EnvVarKey = "CONFIG_ENCRYPTION_KEY";
        public const string EnvVarKeyFile = "CONFIG_ENCRYPTION_KEY_FILE";

        public static string ResolveKey()
        {
            /* priority 1: direct environment variable */
            var key = Environment.GetEnvironmentVariable(EnvVarKey);
            if (!string.IsNullOrEmpty(key))
                return key;

            /* priority 2: file path (Docker secrets, Kubernetes) */
            var keyFile = Environment.GetEnvironmentVariable(EnvVarKeyFile);
            if (!string.IsNullOrEmpty(keyFile) && File.Exists(keyFile))
                return File.ReadAllText(keyFile).Trim();

            return null;
        }
    }
}
