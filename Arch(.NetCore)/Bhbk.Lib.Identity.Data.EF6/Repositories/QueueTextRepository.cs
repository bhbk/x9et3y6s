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
    public class QueueTextRepository : GenericRepository<uvw_QueueTexts>
    {
        public QueueTextRepository(IdentityEntities context, InstanceContext instance)
            : base(context, instance) { }
    }
}
