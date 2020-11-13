using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Claim
    {
        public tbl_Claim()
        {
            tbl_RoleClaims = new HashSet<tbl_RoleClaim>();
            tbl_UserClaims = new HashSet<tbl_UserClaim>();
        }

        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? ActorId { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual tbl_User Actor { get; set; }
        public virtual tbl_Issuer Issuer { get; set; }
        public virtual ICollection<tbl_RoleClaim> tbl_RoleClaims { get; set; }
        public virtual ICollection<tbl_UserClaim> tbl_UserClaims { get; set; }
    }
}
