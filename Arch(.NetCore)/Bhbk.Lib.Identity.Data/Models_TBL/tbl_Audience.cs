using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models_TBL
{
    public partial class tbl_Audience
    {
        public tbl_Audience()
        {
            tbl_Activities = new HashSet<tbl_Activity>();
            tbl_AudienceRoles = new HashSet<tbl_AudienceRole>();
            tbl_Refreshes = new HashSet<tbl_Refresh>();
            tbl_Roles = new HashSet<tbl_Role>();
            tbl_Settings = new HashSet<tbl_Setting>();
            tbl_States = new HashSet<tbl_State>();
            tbl_Urls = new HashSet<tbl_Url>();
        }

        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PasswordHashPBKDF2 { get; set; }
        public string PasswordHashSHA256 { get; set; }
        public string SecurityStamp { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsDeletable { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public DateTimeOffset? LockoutEndUtc { get; set; }
        public DateTimeOffset? LastLoginSuccessUtc { get; set; }
        public DateTimeOffset? LastLoginFailureUtc { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual tbl_Issuer Issuer { get; set; }
        public virtual ICollection<tbl_Activity> tbl_Activities { get; set; }
        public virtual ICollection<tbl_AudienceRole> tbl_AudienceRoles { get; set; }
        public virtual ICollection<tbl_Refresh> tbl_Refreshes { get; set; }
        public virtual ICollection<tbl_Role> tbl_Roles { get; set; }
        public virtual ICollection<tbl_Setting> tbl_Settings { get; set; }
        public virtual ICollection<tbl_State> tbl_States { get; set; }
        public virtual ICollection<tbl_Url> tbl_Urls { get; set; }
    }
}
