using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Issuer
    {
        public tbl_Issuer()
        {
            tbl_Audience = new HashSet<tbl_Audience>();
            tbl_Claim = new HashSet<tbl_Claim>();
            tbl_Refresh = new HashSet<tbl_Refresh>();
            tbl_Setting = new HashSet<tbl_Setting>();
            tbl_State = new HashSet<tbl_State>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IssuerKey { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual ICollection<tbl_Audience> tbl_Audience { get; set; }
        public virtual ICollection<tbl_Claim> tbl_Claim { get; set; }
        public virtual ICollection<tbl_Refresh> tbl_Refresh { get; set; }
        public virtual ICollection<tbl_Setting> tbl_Setting { get; set; }
        public virtual ICollection<tbl_State> tbl_State { get; set; }
    }
}
