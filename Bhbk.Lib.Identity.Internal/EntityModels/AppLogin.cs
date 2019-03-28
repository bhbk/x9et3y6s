using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppLogin
    {
        public AppLogin()
        {
            AppUserLogin = new HashSet<AppUserLogin>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LoginKey { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual ICollection<AppUserLogin> AppUserLogin { get; set; }
    }
}
