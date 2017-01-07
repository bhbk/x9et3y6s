using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Common;
using System.Data.Entity;

namespace Bhbk.Lib.Identity.Infrastructure
{
    //https://www.codeproject.com/Articles/798001/ASP-NET-Identity-Extending-Identity-Models-and-Usi
    public partial class CustomIdentityDbContext : IdentityDbContext<AppUser, AppRole, Guid, AppUserLogin, AppUserRole, AppUserClaim>
    {
        public CustomIdentityDbContext()
            : base("name=IdentityEntities")
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;
        }

        public CustomIdentityDbContext(DbConnection connection)
            : base(connection, true)
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<AppAudience> AppAudience { get; set; }
        public virtual DbSet<AppClient> AppClient { get; set; }
        public virtual DbSet<AppRealm> AppRealm { get; set; }
        public virtual DbSet<AppRole> AppRole { get; set; }
        public virtual DbSet<AppUser> AppUser { get; set; }
        public virtual DbSet<AppUserClaim> AppUserClaim { get; set; }
        public virtual DbSet<AppUserLogin> AppUserLogin { get; set; }
        public virtual DbSet<AppUserRole> AppUserRole { get; set; }
        public virtual DbSet<AppUserToken> AppUserToken { get; set; }
    }
}
