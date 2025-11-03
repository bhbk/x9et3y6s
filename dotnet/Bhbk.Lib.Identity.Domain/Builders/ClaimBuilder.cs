using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using System;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class ClaimBuilder : EntityBuilderBase<ClaimBuilder, tbl_Claim>
    {
        private Guid _issuerId;
        private string _subject;
        private string _type;
        private string _value;
        private string _valueType;
        private bool _isDeletable = true;

        public ClaimBuilder ForIssuer(Guid issuerId)
        {
            _issuerId = issuerId;
            return Self;
        }

        public ClaimBuilder WithSubject(string subject)
        {
            _subject = subject;
            return Self;
        }

        public ClaimBuilder WithType(string type)
        {
            _type = type;
            return Self;
        }

        public ClaimBuilder WithValue(string value)
        {
            _value = value;
            return Self;
        }

        public ClaimBuilder WithValueType(string valueType)
        {
            _valueType = valueType;
            return Self;
        }

        public ClaimBuilder IsDeletable(bool deletable = true)
        {
            _isDeletable = deletable;
            return Self;
        }

        public ClaimBuilder WithDefaults()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            _subject = $"TestSubject-{suffix}";
            _type = $"TestClaim-{suffix}";
            _value = AlphaNumeric.CreateString(8);
            _valueType = "String";
            return Self;
        }

        public override tbl_Claim Build()
        {
            if (string.IsNullOrEmpty(_type))
                throw new InvalidOperationException("Claim type is required");

            if (_issuerId == default)
                throw new InvalidOperationException("IssuerId is required");

            return Map.Map<tbl_Claim>(new ClaimV1
            {
                IssuerId = _issuerId,
                Subject = _subject,
                Type = _type,
                Value = _value ?? AlphaNumeric.CreateString(8),
                ValueType = _valueType,
                IsDeletable = _isDeletable,
            });
        }

        public string GetClaimType() => _type;
    }
}
