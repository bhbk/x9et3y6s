using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Audience
    {
        public tbl_Audience()
        {
            tbl_Activity = new HashSet<tbl_Activity>();
            tbl_AudienceRole = new HashSet<tbl_AudienceRole>();
            tbl_Refresh = new HashSet<tbl_Refresh>();
            tbl_Role = new HashSet<tbl_Role>();
            tbl_Setting = new HashSet<tbl_Setting>();
            tbl_State = new HashSet<tbl_State>();
            tbl_Url = new HashSet<tbl_Url>();
        }

        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PasswordHashPBKDF2 { get; set; }
        public string PasswordHashSHA256 { get; set; }
        public string SecurityStamp { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime? LastLoginSuccess { get; set; }
        public DateTime? LastLoginFailure { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public bool Immutable { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_Issuer Issuer { get; set; }
        public virtual ICollection<tbl_Activity> tbl_Activity { get; set; }
        public virtual ICollection<tbl_AudienceRole> tbl_AudienceRole { get; set; }
        public virtual ICollection<tbl_Refresh> tbl_Refresh { get; set; }
        public virtual ICollection<tbl_Role> tbl_Role { get; set; }
        public virtual ICollection<tbl_Setting> tbl_Setting { get; set; }
        public virtual ICollection<tbl_State> tbl_State { get; set; }
        public virtual ICollection<tbl_Url> tbl_Url { get; set; }
    }
}
