using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_Audience
    {
        public tbl_Audience()
        {
            tbl_AudienceRoles = new HashSet<tbl_AudienceRole>();
            tbl_AudienceAuthActivities = new HashSet<tbl_AudienceAuthActivity>();
            tbl_UserEntitlements = new HashSet<tbl_UserEntitlement>();
            tbl_AudienceEntitlements = new HashSet<tbl_AudienceEntitlement>();
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
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual tbl_Issuer Issuer { get; set; }
        public virtual ICollection<tbl_AudienceRole> tbl_AudienceRoles { get; set; }
        public virtual ICollection<tbl_AudienceAuthActivity> tbl_AudienceAuthActivities { get; set; }
        public virtual ICollection<tbl_UserEntitlement> tbl_UserEntitlements { get; set; }
        public virtual ICollection<tbl_AudienceEntitlement> tbl_AudienceEntitlements { get; set; }
        public virtual ICollection<tbl_Refresh> tbl_Refreshes { get; set; }
        public virtual ICollection<tbl_Role> tbl_Roles { get; set; }
        public virtual ICollection<tbl_Setting> tbl_Settings { get; set; }
        public virtual ICollection<tbl_State> tbl_States { get; set; }
        public virtual ICollection<tbl_Url> tbl_Urls { get; set; }
    }
}
