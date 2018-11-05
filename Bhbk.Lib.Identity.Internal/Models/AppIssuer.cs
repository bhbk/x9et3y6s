using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppIssuer
    {
        public AppIssuer()
        {
            AppClient = new HashSet<AppClient>();
            AppUserRefresh = new HashSet<AppUserRefresh>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IssuerKey { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual ICollection<AppClient> AppClient { get; set; }
        public virtual ICollection<AppUserRefresh> AppUserRefresh { get; set; }
    }
}
