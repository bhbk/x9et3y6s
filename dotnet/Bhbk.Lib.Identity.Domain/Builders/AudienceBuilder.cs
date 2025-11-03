using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Models.Admin;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class AudienceBuilder : EntityBuilderBase<AudienceBuilder, tbl_Audience>
    {
        private Guid _issuerId;
        private string _name;
        private string _description;
        private string _password;
        private bool _isLockedOut = false;
        private bool _isDeletable = true;
        private readonly List<string> _roleNames = new List<string>();

        public AudienceBuilder ForIssuer(Guid issuerId)
        {
            _issuerId = issuerId;
            return Self;
        }

        public AudienceBuilder WithName(string name)
        {
            _name = name;
            return Self;
        }

        public AudienceBuilder WithDescription(string description)
        {
            _description = description;
            return Self;
        }

        public AudienceBuilder WithPassword(string password)
        {
            _password = password;
            return Self;
        }

        public AudienceBuilder IsLockedOut(bool lockedOut = true)
        {
            _isLockedOut = lockedOut;
            return Self;
        }

        public AudienceBuilder IsDeletable(bool deletable = true)
        {
            _isDeletable = deletable;
            return Self;
        }

        public AudienceBuilder WithRole(string roleName)
        {
            _roleNames.Add(roleName);
            return Self;
        }

        public AudienceBuilder WithStandardRoles()
        {
            _roleNames.Add($"{_name}.Admins");
            _roleNames.Add($"{_name}.Users");
            _roleNames.Add($"{_name}.Viewers");
            return Self;
        }

        public AudienceBuilder WithDefaults()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            _name = $"TestAudience-{suffix}";
            _password = AlphaNumeric.CreateString(32);
            return Self;
        }

        public AudienceBuilder WithProductionDefaults()
        {
            _name = "Identity";
            _password = "eBr3r3N1L6JV9jewYJOS6fjZ7EJGeGcb";
            return Self;
        }

        public AudienceBuilder FromTestSettings(TestAudienceSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _name = settings.Name;
            _password = settings.PasswordCurrent;
            return Self;
        }

        public override tbl_Audience Build()
        {
            if (string.IsNullOrEmpty(_name))
                throw new InvalidOperationException("Audience name is required");

            if (_issuerId == default)
                throw new InvalidOperationException("IssuerId is required");

            return Map.Map<tbl_Audience>(new AudienceV1
            {
                IssuerId = _issuerId,
                Name = _name,
                Description = _description,
                IsLockedOut = _isLockedOut,
                IsDeletable = _isDeletable,
            });
        }

        public string GetPassword() => _password ?? AlphaNumeric.CreateString(32);

        public string GetName() => _name;

        public IReadOnlyList<string> GetRoleNames() => _roleNames.AsReadOnly();
    }
}
