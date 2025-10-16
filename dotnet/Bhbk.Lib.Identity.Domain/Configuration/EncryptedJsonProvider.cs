using Bhbk.Lib.Identity.Domain.Encryption;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Configuration
{
    public class EncryptedJsonProvider : JsonConfigurationProvider
    {
        private readonly EncryptionProviderRegistry _registry;

        public EncryptedJsonProvider(JsonConfigurationSource source, EncryptionProviderRegistry registry)
            : base(source)
        {
            _registry = registry;
        }

        public override void Load()
        {
            base.Load();

            if (_registry == null)
                return;

            foreach (var key in Data.Keys.ToList())
            {
                var value = Data[key];

                if (value != null && _registry.IsEncrypted(value))
                {
                    Data[key] = _registry.Decrypt(value);
                }
            }
        }
    }

    public class EncryptedJsonConfigurationSource : JsonConfigurationSource
    {
        public EncryptionProviderRegistry Registry { get; set; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new EncryptedJsonProvider(this, Registry);
        }
    }

    public static class EncryptedJsonConfigurationExtensions
    {
        public static IConfigurationBuilder AddEncryptedJsonFile(
            this IConfigurationBuilder builder,
            string path,
            bool optional = false,
            bool reloadOnChange = false)
        {
            return builder.Add(new EncryptedJsonConfigurationSource
            {
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange,
                Registry = EncryptionProviderRegistry.CreateDefault()
            });
        }

        /* overload for custom registry */
        public static IConfigurationBuilder AddEncryptedJsonFile(
            this IConfigurationBuilder builder,
            string path,
            EncryptionProviderRegistry registry,
            bool optional = false,
            bool reloadOnChange = false)
        {
            return builder.Add(new EncryptedJsonConfigurationSource
            {
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange,
                Registry = registry
            });
        }

        /* backward compatibility overload */
        public static IConfigurationBuilder AddEncryptedJsonFile(
            this IConfigurationBuilder builder,
            string path,
            string encryptionKey,
            bool optional = false,
            bool reloadOnChange = false)
        {
            var registry = new EncryptionProviderRegistry();

            if (!string.IsNullOrEmpty(encryptionKey))
                registry.Register(new AESEncryptionProvider(encryptionKey));

            return builder.Add(new EncryptedJsonConfigurationSource
            {
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange,
                Registry = registry
            });
        }
    }
}
