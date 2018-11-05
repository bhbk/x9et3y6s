using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppUserLogin
    {
        public Guid UserId { get; set; }
        public Guid LoginId { get; set; }
        public Guid? ActorId { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderDescription { get; set; }
        public string ProviderKey { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual AppLogin Login { get; set; }
        public virtual AppUser User { get; set; }
    }
}
