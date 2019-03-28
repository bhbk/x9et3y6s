using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppClaim
    {
        public AppClaim()
        {
            AppRoleClaim = new HashSet<AppRoleClaim>();
            AppUserClaim = new HashSet<AppUserClaim>();
        }

        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? ActorId { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual AppUser Actor { get; set; }
        public virtual AppIssuer Issuer { get; set; }
        public virtual ICollection<AppRoleClaim> AppRoleClaim { get; set; }
        public virtual ICollection<AppUserClaim> AppUserClaim { get; set; }
    }
}
