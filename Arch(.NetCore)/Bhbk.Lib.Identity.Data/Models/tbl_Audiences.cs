using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class tbl_Audiences
    {
        public tbl_Audiences()
        {
            tbl_Activities = new HashSet<tbl_Activities>();
            tbl_AudienceRoles = new HashSet<tbl_AudienceRoles>();
            tbl_Refreshes = new HashSet<tbl_Refreshes>();
            tbl_Roles = new HashSet<tbl_Roles>();
            tbl_Settings = new HashSet<tbl_Settings>();
            tbl_States = new HashSet<tbl_States>();
            tbl_Urls = new HashSet<tbl_Urls>();
        }

        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string AudienceType { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime? LastLoginSuccess { get; set; }
        public DateTime? LastLoginFailure { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Issuers Issuer { get; set; }
        public virtual ICollection<tbl_Activities> tbl_Activities { get; set; }
        public virtual ICollection<tbl_AudienceRoles> tbl_AudienceRoles { get; set; }
        public virtual ICollection<tbl_Refreshes> tbl_Refreshes { get; set; }
        public virtual ICollection<tbl_Roles> tbl_Roles { get; set; }
        public virtual ICollection<tbl_Settings> tbl_Settings { get; set; }
        public virtual ICollection<tbl_States> tbl_States { get; set; }
        public virtual ICollection<tbl_Urls> tbl_Urls { get; set; }
    }
}
