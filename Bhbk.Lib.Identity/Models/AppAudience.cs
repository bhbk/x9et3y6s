using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppAudience
    {
        public AppAudience()
        {
            AppRole = new HashSet<AppRole>();
            AppUserRefresh = new HashSet<AppUserRefresh>();
        }

        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AudienceKey { get; set; }
        public string AudienceType { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public AppClient Client { get; set; }
        public ICollection<AppRole> AppRole { get; set; }
        public ICollection<AppUserRefresh> AppUserRefresh { get; set; }
    }
}
