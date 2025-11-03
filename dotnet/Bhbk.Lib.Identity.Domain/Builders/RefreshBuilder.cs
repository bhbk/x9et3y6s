using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class RefreshBuilder : EntityBuilderBase<RefreshBuilder, tbl_Refresh>
    {
        private Guid _issuerId;
        private Guid? _audienceId;
        private Guid? _userId;
        private string _refreshType;
        private string _refreshValue;
        private DateTime? _validFrom;
        private DateTime? _validTo;

        public RefreshBuilder ForIssuer(Guid issuerId)
        {
            _issuerId = issuerId;
            return Self;
        }

        public RefreshBuilder ForAudience(Guid audienceId)
        {
            _audienceId = audienceId;
            return Self;
        }

        public RefreshBuilder ForUser(Guid userId)
        {
            _userId = userId;
            return Self;
        }

        public RefreshBuilder WithRefreshType(ConsumerType type)
        {
            _refreshType = type.ToString();
            return Self;
        }

        public RefreshBuilder WithRefreshValue(string value)
        {
            _refreshValue = value;
            return Self;
        }

        public RefreshBuilder WithValidity(DateTime from, DateTime to)
        {
            _validFrom = from;
            _validTo = to;
            return Self;
        }

        public RefreshBuilder WithDefaults()
        {
            _refreshType = ConsumerType.User.ToString();
            _refreshValue = Base64.CreateString(32);
            _validFrom = DateTime.UtcNow;
            _validTo = DateTime.UtcNow.AddSeconds(60);
            return Self;
        }

        public override tbl_Refresh Build()
        {
            if (_issuerId == default)
                throw new InvalidOperationException("IssuerId is required");

            return Map.Map<tbl_Refresh>(new RefreshV1
            {
                IssuerId = _issuerId,
                AudienceId = _audienceId,
                UserId = _userId,
                RefreshType = _refreshType ?? ConsumerType.User.ToString(),
                RefreshValue = _refreshValue ?? Base64.CreateString(32),
                ValidFrom = _validFrom ?? DateTime.UtcNow,
                ValidTo = _validTo ?? DateTime.UtcNow.AddSeconds(60),
            });
        }
    }
}
