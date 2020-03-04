using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
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
                    ConfigKey = Constants.ApiSettingAccessExpire,
                    ConfigValue = 600.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            issuer.tbl_Settings.Add(
                new tbl_Settings()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.ApiSettingRefreshExpire,
                    ConfigValue = 86400.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            issuer.tbl_Settings.Add(
                new tbl_Settings()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.ApiSettingTotpExpire,
                    ConfigValue = 600.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            issuer.tbl_Settings.Add(
                new tbl_Settings()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.ApiSettingPollingMax,
                    ConfigValue = 10.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            return _context.Add(issuer).Entity;
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
                .Where(x => x.Audience.IssuerId == issuer.Id);

            var audiences = _context.Set<tbl_Audiences>()
                .Where(x => x.IssuerId == issuer.Id);

            _context.RemoveRange(claims);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);
            _context.RemoveRange(roles);
            _context.RemoveRange(audiences);

            return _context.Remove(issuer).Entity;
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

            return _context.Update(entity).Entity;
        }
    }
}
