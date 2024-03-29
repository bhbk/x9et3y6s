﻿using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data_EF6.Models_Tbl;
using System;
using System.Data.Entity;
using System.Linq;

namespace Bhbk.Lib.Identity.Data_EF6.Repositories_Tbl
{
    public class AudienceRepository : GenericRepository<tbl_Audience>
    {
        private IClockService _clock;

        public AudienceRepository(IdentityEntities context, IContextService env)
            : base(context)
        {
            _clock = new ClockService(env);
        }

        public DateTimeOffset Clock
        {
            get { return _clock.UtcNow; }
            set { _clock.UtcNow = value; }
        }

        public override tbl_Audience Update(tbl_Audience audience)
        {
            var entity = _context.Set<tbl_Audience>()
                .Where(x => x.Id == audience.Id).Single();

            /*
             * only persist certain fields.
             */
            entity.IssuerId = audience.IssuerId;
            entity.Name = audience.Name;
            entity.Description = audience.Description;
            entity.IsDeletable = audience.IsDeletable;

            _context.Entry(entity).State = EntityState.Modified;

            return entity;
        }
    }
}
