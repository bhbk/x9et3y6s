using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppUserLogin
    {
        public Guid UserId { get; set; }
        public Guid LoginId { get; set; }
        public Guid? ActorId { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual AppLogin Login { get; set; }
        public virtual AppUser User { get; set; }
    }
}
