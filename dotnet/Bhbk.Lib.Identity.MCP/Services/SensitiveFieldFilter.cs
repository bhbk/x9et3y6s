using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.MCP.Services
{
    public static class SensitiveFieldFilter
    {
        private static readonly HashSet<string> SensitiveFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PasswordHashPBKDF2",
            "PasswordHashSHA256",
            "SecurityStamp",
            "ConcurrencyStamp",
            "IssuerKey",
            "ProviderKey",
            "ClientSecret",
            "RefreshToken",
            "AccessToken"
        };

        public static JToken Filter(JToken token)
        {
            if (token == null)
                return null;

            switch (token.Type)
            {
                case JTokenType.Object:
                    return FilterObject((JObject)token);
                case JTokenType.Array:
                    return FilterArray((JArray)token);
                default:
                    return token.DeepClone();
            }
        }

        private static JObject FilterObject(JObject obj)
        {
            var result = new JObject();

            foreach (var property in obj.Properties())
            {
                if (SensitiveFields.Contains(property.Name))
                    continue;

                result[property.Name] = Filter(property.Value);
            }

            return result;
        }

        private static JArray FilterArray(JArray array)
        {
            var result = new JArray();

            foreach (var item in array)
            {
                result.Add(Filter(item));
            }

            return result;
        }

        public static bool IsSensitive(string fieldName)
        {
            return SensitiveFields.Contains(fieldName);
        }

        public static void AddSensitiveField(string fieldName)
        {
            SensitiveFields.Add(fieldName);
        }
    }
}
