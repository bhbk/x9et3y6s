using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using System;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class UrlBuilder : EntityBuilderBase<UrlBuilder, tbl_Url>
    {
        private Guid _audienceId;
        private string _urlHost;
        private string _urlPath;
        private bool _isEnabled = true;
        private bool _isDeletable = true;

        public UrlBuilder ForAudience(Guid audienceId)
        {
            _audienceId = audienceId;
            return Self;
        }

        public UrlBuilder WithHost(string host)
        {
            _urlHost = host;
            return Self;
        }

        public UrlBuilder WithPath(string path)
        {
            _urlPath = path;
            return Self;
        }

        public UrlBuilder IsEnabled(bool enabled = true)
        {
            _isEnabled = enabled;
            return Self;
        }

        public UrlBuilder IsDeletable(bool deletable = true)
        {
            _isDeletable = deletable;
            return Self;
        }

        public UrlBuilder WithDefaults()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            _urlHost = $"https://test-{suffix}.local";
            _urlPath = "/callback";
            return Self;
        }

        public override tbl_Url Build()
        {
            if (_audienceId == default)
                throw new InvalidOperationException("AudienceId is required");

            return Map.Map<tbl_Url>(new UrlV1
            {
                AudienceId = _audienceId,
                UrlHost = _urlHost ?? "https://test.local",
                UrlPath = _urlPath ?? "/callback",
                IsEnabled = _isEnabled,
                IsDeletable = _isDeletable,
            });
        }

        public string GetFullUrl() => (_urlHost ?? "https://test.local") + (_urlPath ?? "/callback");
    }
}
