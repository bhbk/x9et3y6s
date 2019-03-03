using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppRoleClaim
    {
        public int Id { get; set; }
        public Guid RoleId { get; set; }
        public Guid? ActorId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public string ClaimTypeValue { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual AppRole Role { get; set; }
    }
}
