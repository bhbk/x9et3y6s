﻿using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.Identity.Data.EF6.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Identity.Data.EF6.Repositories
{
    public class QueueEmailRepository : GenericRepository<uvw_QueueEmails>
    {
        public QueueEmailRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }
    }
}
