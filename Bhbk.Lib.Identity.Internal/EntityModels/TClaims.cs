using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TClaims
    {
        public TClaims()
        {
            TRoleClaims = new HashSet<TRoleClaims>();
            TUserClaims = new HashSet<TUserClaims>();
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

        public virtual TUsers Actor { get; set; }
        public virtual TIssuers Issuer { get; set; }
        public virtual ICollection<TRoleClaims> TRoleClaims { get; set; }
        public virtual ICollection<TUserClaims> TUserClaims { get; set; }
    }
}
