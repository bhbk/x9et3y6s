using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Models.Admin;
using System;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class RoleBuilder : EntityBuilderBase<RoleBuilder, tbl_Role>
    {
        private Guid _audienceId;
        private string _name;
        private string _description;
        private bool _isEnabled = true;
        private bool _isDeletable = true;

        public RoleBuilder ForAudience(Guid audienceId)
        {
            _audienceId = audienceId;
            return Self;
        }

        public RoleBuilder WithName(string name)
        {
            _name = name;
            return Self;
        }

        public RoleBuilder WithDescription(string description)
        {
            _description = description;
            return Self;
        }

        public RoleBuilder IsEnabled(bool enabled = true)
        {
            _isEnabled = enabled;
            return Self;
        }

        public RoleBuilder IsDeletable(bool deletable = true)
        {
            _isDeletable = deletable;
            return Self;
        }

        public RoleBuilder WithDefaults()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            _name = $"TestRole-{suffix}";
            return Self;
        }

        public RoleBuilder FromTestSettings(TestRoleSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _name = settings.Name;
            return Self;
        }

        public override tbl_Role Build()
        {
            if (string.IsNullOrEmpty(_name))
                throw new InvalidOperationException("Role name is required");

            if (_audienceId == default)
                throw new InvalidOperationException("AudienceId is required");

            return Map.Map<tbl_Role>(new RoleV1
            {
                AudienceId = _audienceId,
                Name = _name,
                Description = _description,
                IsEnabled = _isEnabled,
                IsDeletable = _isDeletable,
            });
        }

        public string GetName() => _name;
    }
}
