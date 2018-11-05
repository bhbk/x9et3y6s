using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppRole
    {
        public AppRole()
        {
            AppRoleClaim = new HashSet<AppRoleClaim>();
            AppUserRole = new HashSet<AppUserRole>();
        }

        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string ConcurrencyStamp { get; set; }
        public bool Immutable { get; set; }

        public virtual AppClient Client { get; set; }
        public virtual ICollection<AppRoleClaim> AppRoleClaim { get; set; }
        public virtual ICollection<AppUserRole> AppUserRole { get; set; }
    }
}
