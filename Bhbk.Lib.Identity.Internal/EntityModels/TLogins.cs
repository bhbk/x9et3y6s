using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TLogins
    {
        public TLogins()
        {
            TUserLogins = new HashSet<TUserLogins>();
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

        public virtual TUsers Actor { get; set; }
        public virtual ICollection<TUserLogins> TUserLogins { get; set; }
    }
}
