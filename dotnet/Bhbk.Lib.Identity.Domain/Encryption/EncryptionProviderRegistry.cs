using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Encryption
{
    public class EncryptionProviderRegistry
    {
        private readonly List<IConfigEncryptionProvider> _providers = new();

        public void Register(IConfigEncryptionProvider provider)
        {
            _providers.Add(provider);
        }

        public string Decrypt(string prefixedValue)
        {
            var provider = _providers.FirstOrDefault(p => p.CanDecrypt(prefixedValue));
            if (provider == null)
                throw new InvalidOperationException($"No provider registered for: {prefixedValue.Substring(0, Math.Min(10, prefixedValue.Length))}...");

            var ciphertext = prefixedValue.Substring(provider.Prefix.Length);
            return provider.Decrypt(ciphertext);
        }

        public bool IsEncrypted(string value)
            => _providers.Any(p => p.CanDecrypt(value));

        public static EncryptionProviderRegistry CreateDefault()
        {
            var registry = new EncryptionProviderRegistry();
            var key = EncryptionKeyResolver.ResolveKey();

            if (!string.IsNullOrEmpty(key))
                registry.Register(new AESEncryptionProvider(key));

            return registry;
        }
    }
}
