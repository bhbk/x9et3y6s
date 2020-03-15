﻿using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories
{
    public class QueueEmailRepository : GenericRepository<uvw_QueueEmails>
    {
        public QueueEmailRepository(IdentityEntities context)
            : base(context) { }
    }
}