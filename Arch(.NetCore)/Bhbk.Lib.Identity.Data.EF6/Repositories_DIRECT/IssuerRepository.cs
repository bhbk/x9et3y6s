using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using Bhbk.Lib.Identity.Primitives;
using System;
using System.Data.Entity;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories_DIRECT
{
    public class IssuerRepository : GenericRepository<tbl_Issuers>
    {
        public IssuerRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Issuers Create(tbl_Issuers issuer)
        {
            issuer.tbl_Settings.Add(
                new tbl_Settings()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingAccessExpire,
                    ConfigValue = 600.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            issuer.tbl_Settings.Add(
                new tbl_Settings()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingRefreshExpire,
                    ConfigValue = 86400.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            issuer.tbl_Settings.Add(
                new tbl_Settings()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingTotpExpire,
                    ConfigValue = 600.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            issuer.tbl_Settings.Add(
                new tbl_Settings()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingPollingMax,
                    ConfigValue = 10.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            return _context.Set<tbl_Issuers>().Add(issuer);
        }

        public override tbl_Issuers Delete(tbl_Issuers issuer)
        {            
            var claims = _context.Set<tbl_Claims>()
                .Where(x => x.IssuerId == issuer.Id);

            var refreshes = _context.Set<tbl_Refreshes>()
                .Where(x => x.IssuerId == issuer.Id);

            var settings = _context.Set<tbl_Settings>()
                .Where(x => x.IssuerId == issuer.Id);

            var states = _context.Set<tbl_States>()
                .Where(x => x.IssuerId == issuer.Id);

            var roles = _context.Set<tbl_Roles>()
                .Where(x => x.tbl_Audiences.IssuerId == issuer.Id);

            var audiences = _context.Set<tbl_Audiences>()
                .Where(x => x.IssuerId == issuer.Id);

            _context.Set<tbl_Claims>().RemoveRange(claims);
            _context.Set<tbl_Refreshes>().RemoveRange(refreshes);
            _context.Set<tbl_Settings>().RemoveRange(settings);
            _context.Set<tbl_States>().RemoveRange(states);
            _context.Set<tbl_Roles>().RemoveRange(roles);
            _context.Set<tbl_Audiences>().RemoveRange(audiences);

            return _context.Set<tbl_Issuers>().Remove(issuer);
        }

        public override tbl_Issuers Update(tbl_Issuers issuer)
        {
            var entity = _context.Set<tbl_Issuers>()
                .Where(x => x.Id == issuer.Id).Single();

            /*
             * only persist certain fields.
             */

            entity.Name = issuer.Name;
            entity.Description = issuer.Description;
            entity.IssuerKey = issuer.IssuerKey;
            entity.LastUpdated = DateTime.Now;
            entity.Enabled = issuer.Enabled;
            entity.Immutable = issuer.Immutable;

            _context.Entry(entity).State = EntityState.Modified;

            return entity;
        }
    }
}
