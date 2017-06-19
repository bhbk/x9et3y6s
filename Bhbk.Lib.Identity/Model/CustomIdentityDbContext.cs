using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Common;
using System.Data.Entity;

namespace Bhbk.Lib.Identity.Model
{
    //https://www.codeproject.com/Articles/798001/ASP-NET-Identity-Extending-Identity-Models-and-Usi
    public partial class CustomIdentityDbContext : IdentityDbContext<AppUser, AppRole, Guid, AppUserProvider, AppUserRole, AppUserClaim>
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
        public virtual DbSet<AppProvider> AppProvider { get; set; }
        public virtual DbSet<AppRole> AppRole { get; set; }
        public virtual DbSet<AppUser> AppUser { get; set; }
        public virtual DbSet<AppUserClaim> AppUserClaim { get; set; }
        public virtual DbSet<AppUserProvider> AppUserProvider { get; set; }
        public virtual DbSet<AppUserRefreshToken> AppUserRefreshToken { get; set; }
        public virtual DbSet<AppUserRole> AppUserRole { get; set; }
    }
}
