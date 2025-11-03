using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Alert;
using System;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class TextBuilder : EntityBuilderBase<TextBuilder, tbl_TextQueue>
    {
        private string _fromPhoneNumber;
        private string _toPhoneNumber;
        private string _body;
        private DateTime? _sendAt;

        public TextBuilder WithFromPhoneNumber(string fromPhoneNumber)
        {
            _fromPhoneNumber = fromPhoneNumber;
            return Self;
        }

        public TextBuilder WithToPhoneNumber(string toPhoneNumber)
        {
            _toPhoneNumber = toPhoneNumber;
            return Self;
        }

        public TextBuilder WithBody(string body)
        {
            _body = body;
            return Self;
        }

        public TextBuilder WithSendAt(DateTime sendAt)
        {
            _sendAt = sendAt;
            return Self;
        }

        public TextBuilder WithDefaults()
        {
            _fromPhoneNumber = NumberAs.CreateString(11);
            _toPhoneNumber = NumberAs.CreateString(11);
            _body = $"Body-{Base64.CreateString(32)}";
            _sendAt = DateTime.UtcNow;
            return Self;
        }

        public override tbl_TextQueue Build()
        {
            if (string.IsNullOrEmpty(_fromPhoneNumber))
                throw new InvalidOperationException("FromPhoneNumber is required");

            if (string.IsNullOrEmpty(_toPhoneNumber))
                throw new InvalidOperationException("ToPhoneNumber is required");

            return Map.Map<tbl_TextQueue>(new TextV1
            {
                FromPhoneNumber = _fromPhoneNumber,
                ToPhoneNumber = _toPhoneNumber,
                Body = _body ?? $"Body-{AlphaNumeric.CreateString(32)}",
                SendAt = _sendAt ?? DateTime.UtcNow,
            });
        }
    }
}
