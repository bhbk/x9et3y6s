using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Alert;
using System;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class EmailBuilder : EntityBuilderBase<EmailBuilder, tbl_EmailQueue>
    {
        private string _fromEmail;
        private string _toEmail;
        private string _subject;
        private string _body;
        private DateTime? _sendAt;

        public EmailBuilder WithFromEmail(string fromEmail)
        {
            _fromEmail = fromEmail;
            return Self;
        }

        public EmailBuilder WithToEmail(string toEmail)
        {
            _toEmail = toEmail;
            return Self;
        }

        public EmailBuilder WithSubject(string subject)
        {
            _subject = subject;
            return Self;
        }

        public EmailBuilder WithBody(string body)
        {
            _body = body;
            return Self;
        }

        public EmailBuilder WithSendAt(DateTime sendAt)
        {
            _sendAt = sendAt;
            return Self;
        }

        public EmailBuilder WithDefaults()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            _fromEmail = $"from-{suffix}@test.local";
            _toEmail = $"to-{suffix}@test.local";
            _subject = $"Subject-{suffix}";
            _body = $"Body-{Base64.CreateString(32)}";
            _sendAt = DateTime.UtcNow;
            return Self;
        }

        public override tbl_EmailQueue Build()
        {
            if (string.IsNullOrEmpty(_fromEmail))
                throw new InvalidOperationException("FromEmail is required");

            if (string.IsNullOrEmpty(_toEmail))
                throw new InvalidOperationException("ToEmail is required");

            return Map.Map<tbl_EmailQueue>(new EmailV1
            {
                FromEmail = _fromEmail,
                ToEmail = _toEmail,
                Subject = _subject ?? $"Subject-{AlphaNumeric.CreateString(4)}",
                Body = _body ?? $"Body-{AlphaNumeric.CreateString(32)}",
                SendAt = _sendAt ?? DateTime.UtcNow,
            });
        }
    }
}
