using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Claim
    {
        public tbl_Claim()
        {
            tbl_RoleClaim = new HashSet<tbl_RoleClaim>();
            tbl_UserClaim = new HashSet<tbl_UserClaim>();
        }

        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? ActorId { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public bool Immutable { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_User Actor { get; set; }
        public virtual tbl_Issuer Issuer { get; set; }
        public virtual ICollection<tbl_RoleClaim> tbl_RoleClaim { get; set; }
        public virtual ICollection<tbl_UserClaim> tbl_UserClaim { get; set; }
    }
}
