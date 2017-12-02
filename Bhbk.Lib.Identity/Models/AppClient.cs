using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppClient
    {
        public AppClient()
        {
            AppAudience = new HashSet<AppAudience>();
            AppUserRefresh = new HashSet<AppUserRefresh>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public ICollection<AppAudience> AppAudience { get; set; }
        public ICollection<AppUserRefresh> AppUserRefresh { get; set; }
    }
}
