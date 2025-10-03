using Bhbk.Lib.Identity.MCP.Services;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF.Tests.MCPTests
{
    [Collection("MCPTests")]
    public class SensitiveFieldFilterTests
    {
        [Fact]
        public void Filter_RemovesPasswordHash()
        {
            var data = new JObject
            {
                ["Id"] = "123",
                ["UserName"] = "testuser",
                ["PasswordHashPBKDF2"] = "sensitive_hash_data"
            };

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().ContainKey("Id");
            result.Should().ContainKey("UserName");
            result.Should().NotContainKey("PasswordHashPBKDF2");
        }

        [Fact]
        public void Filter_RemovesSHA256Hash()
        {
            var data = new JObject
            {
                ["Id"] = "123",
                ["PasswordHashSHA256"] = "sha256_sensitive_data"
            };

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().ContainKey("Id");
            result.Should().NotContainKey("PasswordHashSHA256");
        }

        [Fact]
        public void Filter_RemovesSecurityStamp()
        {
            var data = new JObject
            {
                ["Id"] = "123",
                ["SecurityStamp"] = "security_stamp_value",
                ["ConcurrencyStamp"] = "concurrency_stamp_value"
            };

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().ContainKey("Id");
            result.Should().NotContainKey("SecurityStamp");
            result.Should().NotContainKey("ConcurrencyStamp");
        }

        [Fact]
        public void Filter_RemovesIssuerKey()
        {
            var data = new JObject
            {
                ["Id"] = "123",
                ["Name"] = "TestIssuer",
                ["IssuerKey"] = "secret_issuer_key"
            };

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().ContainKey("Id");
            result.Should().ContainKey("Name");
            result.Should().NotContainKey("IssuerKey");
        }

        [Fact]
        public void Filter_RemovesProviderKey()
        {
            var data = new JObject
            {
                ["Id"] = "123",
                ["Name"] = "TestLoginProvider",
                ["ProviderKey"] = "secret_provider_key"
            };

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().ContainKey("Id");
            result.Should().ContainKey("Name");
            result.Should().NotContainKey("ProviderKey");
        }

        [Fact]
        public void Filter_RemovesClientSecret()
        {
            var data = new JObject
            {
                ["Id"] = "123",
                ["ClientId"] = "client123",
                ["ClientSecret"] = "super_secret_client"
            };

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().ContainKey("Id");
            result.Should().ContainKey("ClientId");
            result.Should().NotContainKey("ClientSecret");
        }

        [Fact]
        public void Filter_RemovesTokens()
        {
            var data = new JObject
            {
                ["Id"] = "123",
                ["RefreshToken"] = "refresh_token_value",
                ["AccessToken"] = "access_token_value"
            };

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().ContainKey("Id");
            result.Should().NotContainKey("RefreshToken");
            result.Should().NotContainKey("AccessToken");
        }

        [Fact]
        public void Filter_HandlesNestedObjects()
        {
            var data = new JObject
            {
                ["Id"] = "123",
                ["User"] = new JObject
                {
                    ["UserName"] = "testuser",
                    ["PasswordHashPBKDF2"] = "nested_password_hash"
                }
            };

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().ContainKey("Id");
            result.Should().ContainKey("User");

            var nestedUser = result["User"] as JObject;
            nestedUser.Should().NotBeNull();
            nestedUser.Should().ContainKey("UserName");
            nestedUser.Should().NotContainKey("PasswordHashPBKDF2");
        }

        [Fact]
        public void Filter_HandlesArrays()
        {
            var data = new JObject
            {
                ["Users"] = new JArray
                {
                    new JObject
                    {
                        ["Id"] = "1",
                        ["PasswordHashPBKDF2"] = "hash1"
                    },
                    new JObject
                    {
                        ["Id"] = "2",
                        ["SecurityStamp"] = "stamp2"
                    }
                }
            };

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().ContainKey("Users");
            var users = result["Users"] as JArray;
            users.Should().NotBeNull();
            users.Should().HaveCount(2);

            var user1 = users[0] as JObject;
            user1.Should().ContainKey("Id");
            user1.Should().NotContainKey("PasswordHashPBKDF2");

            var user2 = users[1] as JObject;
            user2.Should().ContainKey("Id");
            user2.Should().NotContainKey("SecurityStamp");
        }

        [Fact]
        public void Filter_PreservesNonSensitiveFields()
        {
            var data = new JObject
            {
                ["Id"] = "123",
                ["UserName"] = "testuser",
                ["Email"] = "test@example.com",
                ["FirstName"] = "Test",
                ["LastName"] = "User",
                ["IsEnabled"] = true,
                ["CreatedUtc"] = "2024-01-01T00:00:00Z"
            };

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().HaveCount(7);
            result.Should().ContainKey("Id");
            result.Should().ContainKey("UserName");
            result.Should().ContainKey("Email");
            result.Should().ContainKey("FirstName");
            result.Should().ContainKey("LastName");
            result.Should().ContainKey("IsEnabled");
            result.Should().ContainKey("CreatedUtc");
        }

        [Fact]
        public void Filter_HandlesEmptyObject()
        {
            var data = new JObject();

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().BeEmpty();
        }

        [Fact]
        public void Filter_HandlesNullValues()
        {
            var data = new JObject
            {
                ["Id"] = "123",
                ["Email"] = JValue.CreateNull(),
                ["PasswordHashPBKDF2"] = JValue.CreateNull()
            };

            var result = SensitiveFieldFilter.Filter(data) as JObject;

            result.Should().ContainKey("Id");
            result.Should().ContainKey("Email");
            result.Should().NotContainKey("PasswordHashPBKDF2");
        }

        [Fact]
        public void IsSensitive_ReturnsTrueForSensitiveFields()
        {
            SensitiveFieldFilter.IsSensitive("PasswordHashPBKDF2").Should().BeTrue();
            SensitiveFieldFilter.IsSensitive("SecurityStamp").Should().BeTrue();
            SensitiveFieldFilter.IsSensitive("IssuerKey").Should().BeTrue();
        }

        [Fact]
        public void IsSensitive_ReturnsFalseForNonSensitiveFields()
        {
            SensitiveFieldFilter.IsSensitive("Id").Should().BeFalse();
            SensitiveFieldFilter.IsSensitive("UserName").Should().BeFalse();
            SensitiveFieldFilter.IsSensitive("Email").Should().BeFalse();
        }

        [Fact]
        public void IsSensitive_IsCaseInsensitive()
        {
            SensitiveFieldFilter.IsSensitive("passwordhashpbkdf2").Should().BeTrue();
            SensitiveFieldFilter.IsSensitive("SECURITYSTAMP").Should().BeTrue();
            SensitiveFieldFilter.IsSensitive("issuerKEY").Should().BeTrue();
        }
    }
}
