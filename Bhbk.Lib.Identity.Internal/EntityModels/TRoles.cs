using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TRoles
    {
        public TRoles()
        {
            TRoleClaims = new HashSet<TRoleClaims>();
            TUserRoles = new HashSet<TUserRoles>();
        }

        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string ConcurrencyStamp { get; set; }
        public bool Immutable { get; set; }

        public virtual TClients Client { get; set; }
        public virtual ICollection<TRoleClaims> TRoleClaims { get; set; }
        public virtual ICollection<TUserRoles> TUserRoles { get; set; }
    }
}
