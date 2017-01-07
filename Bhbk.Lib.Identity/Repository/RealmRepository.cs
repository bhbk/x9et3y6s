﻿using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Repository
{
    //https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
    public class RealmRepository : GenericRepository<AppRealm, Guid>
    {
        public RealmRepository(CustomIdentityDbContext context)
            : base(context) { }

        public override bool Exists(Guid key)
        {
            return _context.AppRealm.Any(x => x.Id == key);
        }
    }
}
