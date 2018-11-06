using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.EntityModels
{
    public partial class AppUserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public Guid? ActorId { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual AppRole Role { get; set; }
        public virtual AppUser User { get; set; }
    }
}
