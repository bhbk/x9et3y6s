using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Issuer
    {
        public tbl_Issuer()
        {
            tbl_Audiences = new HashSet<tbl_Audience>();
            tbl_Claims = new HashSet<tbl_Claim>();
            tbl_Refreshes = new HashSet<tbl_Refresh>();
            tbl_Settings = new HashSet<tbl_Setting>();
            tbl_States = new HashSet<tbl_State>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IssuerKey { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual ICollection<tbl_Audience> tbl_Audiences { get; set; }
        public virtual ICollection<tbl_Claim> tbl_Claims { get; set; }
        public virtual ICollection<tbl_Refresh> tbl_Refreshes { get; set; }
        public virtual ICollection<tbl_Setting> tbl_Settings { get; set; }
        public virtual ICollection<tbl_State> tbl_States { get; set; }
    }
}
