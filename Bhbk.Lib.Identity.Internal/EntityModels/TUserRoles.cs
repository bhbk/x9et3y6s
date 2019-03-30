using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TUserRoles
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public Guid? ActorId { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual TRoles Role { get; set; }
        public virtual TUsers User { get; set; }
    }
}
