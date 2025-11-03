using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Me;
using System;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class QuoteBuilder : EntityBuilderBase<QuoteBuilder, tbl_Quote>
    {
        private string _author;
        private string _quote;
        private string _category;
        private string _title;

        public QuoteBuilder WithAuthor(string author)
        {
            _author = author;
            return Self;
        }

        public QuoteBuilder WithQuote(string quote)
        {
            _quote = quote;
            return Self;
        }

        public QuoteBuilder WithCategory(string category)
        {
            _category = category;
            return Self;
        }

        public QuoteBuilder WithTitle(string title)
        {
            _title = title;
            return Self;
        }

        public QuoteBuilder WithDefaults()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            _author = $"TestAuthor-{suffix}";
            _quote = $"Quote-{Base64.CreateString(32)}";
            _category = "Test Category";
            _title = "Test Title";
            return Self;
        }

        public override tbl_Quote Build()
        {
            if (string.IsNullOrEmpty(_author))
                throw new InvalidOperationException("Author is required");

            return Map.Map<tbl_Quote>(new QuoteV1
            {
                globalId = Guid.NewGuid(),
                author = _author,
                quote = _quote ?? $"Quote-{AlphaNumeric.CreateString(32)}",
                length = (_quote ?? "").Length.ToString(),
                id = AlphaNumeric.CreateString(8),
                date = DateTime.UtcNow.ToString(),
                category = _category ?? "Test Category",
                title = _title ?? "Test Title",
                background = "Test Background",
                tags = new System.Collections.Generic.List<string> { "tag1", "tag2", "tag3" },
            });
        }

        public string GetAuthor() => _author;
    }
}
