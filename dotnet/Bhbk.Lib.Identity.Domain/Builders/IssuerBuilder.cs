using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Constants;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class IssuerBuilder : EntityBuilderBase<IssuerBuilder, tbl_Issuer>
    {
        private string _name;
        private string _description;
        private string _issuerKey;
        private bool _isEnabled = true;
        private bool _isDeletable = true;
        private int? _accessExpire;
        private int? _refreshExpire;
        private int? _totpExpire;
        private int? _pollingMax;

        public IssuerBuilder WithName(string name)
        {
            _name = name;
            return Self;
        }

        public IssuerBuilder WithDescription(string description)
        {
            _description = description;
            return Self;
        }

        public IssuerBuilder WithIssuerKey(string issuerKey)
        {
            _issuerKey = issuerKey;
            return Self;
        }

        public IssuerBuilder IsEnabled(bool enabled = true)
        {
            _isEnabled = enabled;
            return Self;
        }

        public IssuerBuilder IsDeletable(bool deletable = true)
        {
            _isDeletable = deletable;
            return Self;
        }

        public IssuerBuilder WithAccessExpire(int seconds)
        {
            _accessExpire = seconds;
            return Self;
        }

        public IssuerBuilder WithRefreshExpire(int seconds)
        {
            _refreshExpire = seconds;
            return Self;
        }

        public IssuerBuilder WithTotpExpire(int seconds)
        {
            _totpExpire = seconds;
            return Self;
        }

        public IssuerBuilder WithPollingMax(int count)
        {
            _pollingMax = count;
            return Self;
        }

        public IssuerBuilder WithDefaults()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            _name = $"TestIssuer-{suffix}";
            _issuerKey = AlphaNumeric.CreateString(256);
            _accessExpire = 600;
            _refreshExpire = 86400;
            _totpExpire = 600;
            _pollingMax = 10;
            return Self;
        }

        public IssuerBuilder WithProductionDefaults()
        {
            _name = "Local";
            _issuerKey = "8%8?gh=-:P^&r([7+F(gQP_{g>z,RF2[C:-Dhdd6uMdW@^ocn7(w4_:*hHHWQU2rx-!PBG%;-5:>gw?P+Aq>9-M&adtEmovN4]4M:a5jgNR^Rv![28@aa;EfVU_D9NGPr@X[Y3uz:*5f64G+zA^NUo.efFy_r<hWPt?3%X>Z*Uk<KKNv9^a6W>BLTRH#vuxCe;x8JWY<d*Z6h!LvxEm[H-.zuMf#Z;}jPJ4L;PKewoc_#iGPtYd%W,]3Fo77KSXM";
            _accessExpire = 600;
            _refreshExpire = 86400;
            _totpExpire = 600;
            _pollingMax = 10;
            return Self;
        }

        public IssuerBuilder FromTestSettings(TestIssuerSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _name = settings.Name;
            _issuerKey = settings.IssuerKey;
            _accessExpire = 600;
            _refreshExpire = 86400;
            _totpExpire = 600;
            _pollingMax = 10;
            return Self;
        }

        public override tbl_Issuer Build()
        {
            if (string.IsNullOrEmpty(_name))
                throw new InvalidOperationException("Issuer name is required");

            var entity = Map.Map<tbl_Issuer>(new IssuerV1
            {
                Name = _name,
                Description = _description,
                IsEnabled = _isEnabled,
                IsDeletable = _isDeletable,
            });

            entity.IssuerKey = _issuerKey ?? AlphaNumeric.CreateString(256);

            return entity;
        }

        public List<tbl_Setting> BuildSettings(Guid issuerId)
        {
            var settings = new List<tbl_Setting>();

            if (_accessExpire.HasValue)
            {
                settings.Add(new tbl_Setting
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuerId,
                    ConfigKey = SettingsConstants.AccessExpire,
                    ConfigValue = _accessExpire.Value.ToString(),
                    IsDeletable = true,
                    Created = DateTimeOffset.UtcNow,
                });
            }

            if (_refreshExpire.HasValue)
            {
                settings.Add(new tbl_Setting
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuerId,
                    ConfigKey = SettingsConstants.RefreshExpire,
                    ConfigValue = _refreshExpire.Value.ToString(),
                    IsDeletable = true,
                    Created = DateTimeOffset.UtcNow,
                });
            }

            if (_totpExpire.HasValue)
            {
                settings.Add(new tbl_Setting
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuerId,
                    ConfigKey = SettingsConstants.TotpExpire,
                    ConfigValue = _totpExpire.Value.ToString(),
                    IsDeletable = true,
                    Created = DateTimeOffset.UtcNow,
                });
            }

            if (_pollingMax.HasValue)
            {
                settings.Add(new tbl_Setting
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuerId,
                    ConfigKey = SettingsConstants.PollingMax,
                    ConfigValue = _pollingMax.Value.ToString(),
                    IsDeletable = true,
                    Created = DateTimeOffset.UtcNow,
                });
            }

            return settings;
        }

        public string GetIssuerKey() => _issuerKey;

        public string GetName() => _name;
    }
}
