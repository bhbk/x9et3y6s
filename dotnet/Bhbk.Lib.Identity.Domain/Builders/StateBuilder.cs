using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class StateBuilder : EntityBuilderBase<StateBuilder, tbl_State>
    {
        private Guid _issuerId;
        private Guid? _audienceId;
        private Guid? _userId;
        private string _stateValue;
        private string _stateType;
        private bool _stateConsume;
        private DateTime? _validFrom;
        private DateTime? _validTo;

        public StateBuilder ForIssuer(Guid issuerId)
        {
            _issuerId = issuerId;
            return Self;
        }

        public StateBuilder ForAudience(Guid audienceId)
        {
            _audienceId = audienceId;
            return Self;
        }

        public StateBuilder ForUser(Guid userId)
        {
            _userId = userId;
            return Self;
        }

        public StateBuilder WithStateValue(string value)
        {
            _stateValue = value;
            return Self;
        }

        public StateBuilder WithStateType(ConsumerType type)
        {
            _stateType = type.ToString();
            return Self;
        }

        public StateBuilder WithStateConsume(bool consume)
        {
            _stateConsume = consume;
            return Self;
        }

        public StateBuilder WithValidity(DateTime from, DateTime to)
        {
            _validFrom = from;
            _validTo = to;
            return Self;
        }

        public StateBuilder WithDefaults()
        {
            _stateValue = AlphaNumeric.CreateString(32);
            _stateType = ConsumerType.Device.ToString();
            _stateConsume = false;
            _validFrom = DateTime.UtcNow;
            _validTo = DateTime.UtcNow.AddSeconds(60);
            return Self;
        }

        public override tbl_State Build()
        {
            if (_issuerId == default)
                throw new InvalidOperationException("IssuerId is required");

            return Map.Map<tbl_State>(new StateV1
            {
                IssuerId = _issuerId,
                AudienceId = _audienceId,
                UserId = _userId,
                StateValue = _stateValue ?? AlphaNumeric.CreateString(32),
                StateType = _stateType ?? ConsumerType.Device.ToString(),
                StateConsume = _stateConsume,
                ValidFrom = _validFrom ?? DateTime.UtcNow,
                ValidTo = _validTo ?? DateTime.UtcNow.AddSeconds(60),
            });
        }
    }
}
