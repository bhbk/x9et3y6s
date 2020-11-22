using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_TBL
{
    public partial class tbl_Role
    {
        public tbl_Role()
        {
            tbl_AudienceRoles = new HashSet<tbl_AudienceRole>();
            tbl_RoleClaims = new HashSet<tbl_RoleClaim>();
            tbl_UserRoles = new HashSet<tbl_UserRole>();
        }

        public Guid Id { get; set; }
        public Guid AudienceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual tbl_Audience Audience { get; set; }
        public virtual ICollection<tbl_AudienceRole> tbl_AudienceRoles { get; set; }
        public virtual ICollection<tbl_RoleClaim> tbl_RoleClaims { get; set; }
        public virtual ICollection<tbl_UserRole> tbl_UserRoles { get; set; }
    }
}
