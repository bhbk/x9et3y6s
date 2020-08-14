using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Repositories_DIRECT
{
    public class IssuerRepository : GenericRepository<tbl_Issuer>
    {
        public IssuerRepository(IdentityEntities context)
            : base(context) { }

        public override tbl_Issuer Create(tbl_Issuer issuer)
        {
            issuer.tbl_Setting.Add(
                new tbl_Setting()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingAccessExpire,
                    ConfigValue = 600.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            issuer.tbl_Setting.Add(
                new tbl_Setting()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingRefreshExpire,
                    ConfigValue = 86400.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            issuer.tbl_Setting.Add(
                new tbl_Setting()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingTotpExpire,
                    ConfigValue = 600.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            issuer.tbl_Setting.Add(
                new tbl_Setting()
                {
                    Id = Guid.NewGuid(),
                    IssuerId = issuer.Id,
                    ConfigKey = Constants.SettingPollingMax,
                    ConfigValue = 10.ToString(),
                    Created = DateTime.UtcNow,
                    Immutable = true,
                });

            return _context.Add(issuer).Entity;
        }

        public override tbl_Issuer Delete(tbl_Issuer issuer)
        {            
            var claims = _context.Set<tbl_Claim>()
                .Where(x => x.IssuerId == issuer.Id);

            var refreshes = _context.Set<tbl_Refresh>()
                .Where(x => x.IssuerId == issuer.Id);

            var settings = _context.Set<tbl_Setting>()
                .Where(x => x.IssuerId == issuer.Id);

            var states = _context.Set<tbl_State>()
                .Where(x => x.IssuerId == issuer.Id);

            var roles = _context.Set<tbl_Role>()
                .Where(x => x.Audience.IssuerId == issuer.Id);

            var audiences = _context.Set<tbl_Audience>()
                .Where(x => x.IssuerId == issuer.Id);

            _context.RemoveRange(claims);
            _context.RemoveRange(refreshes);
            _context.RemoveRange(settings);
            _context.RemoveRange(states);
            _context.RemoveRange(roles);
            _context.RemoveRange(audiences);

            return _context.Remove(issuer).Entity;
        }

        public override tbl_Issuer Update(tbl_Issuer issuer)
        {
            var entity = _context.Set<tbl_Issuer>()
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
