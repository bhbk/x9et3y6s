using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Models.Admin;
using System;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class LoginProviderBuilder : EntityBuilderBase<LoginProviderBuilder, tbl_LoginProvider>
    {
        private string _name;
        private string _description;
        private string _providerKey;
        private bool _isEnabled = true;
        private bool _isDeletable = true;

        public LoginProviderBuilder WithName(string name)
        {
            _name = name;
            return Self;
        }

        public LoginProviderBuilder WithDescription(string description)
        {
            _description = description;
            return Self;
        }

        public LoginProviderBuilder WithProviderKey(string providerKey)
        {
            _providerKey = providerKey;
            return Self;
        }

        public LoginProviderBuilder IsEnabled(bool enabled = true)
        {
            _isEnabled = enabled;
            return Self;
        }

        public LoginProviderBuilder IsDeletable(bool deletable = true)
        {
            _isDeletable = deletable;
            return Self;
        }

        public LoginProviderBuilder WithDefaults()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            _name = $"TestLoginProvider-{suffix}";
            _providerKey = AlphaNumeric.CreateString(16);
            return Self;
        }

        public LoginProviderBuilder FromTestSettings(TestLoginProviderSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _name = settings.Name;
            _providerKey = settings.ProviderKey;
            return Self;
        }

        public override tbl_LoginProvider Build()
        {
            if (string.IsNullOrEmpty(_name))
                throw new InvalidOperationException("LoginProvider name is required");

            var entity = Map.Map<tbl_LoginProvider>(new LoginProviderV1
            {
                Name = _name,
                Description = _description,
                IsEnabled = _isEnabled,
                IsDeletable = _isDeletable,
            });

            entity.ProviderKey = _providerKey ?? AlphaNumeric.CreateString(16);

            return entity;
        }

        public string GetName() => _name;
    }
}
