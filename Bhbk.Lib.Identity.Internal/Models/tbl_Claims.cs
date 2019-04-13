using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.Models
{
    public partial class tbl_Claims
    {
        public tbl_Claims()
        {
            tbl_RoleClaims = new HashSet<tbl_RoleClaims>();
            tbl_UserClaims = new HashSet<tbl_UserClaims>();
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

        public virtual tbl_Users Actor { get; set; }
        public virtual tbl_Issuers Issuer { get; set; }
        public virtual ICollection<tbl_RoleClaims> tbl_RoleClaims { get; set; }
        public virtual ICollection<tbl_UserClaims> tbl_UserClaims { get; set; }
    }
}
