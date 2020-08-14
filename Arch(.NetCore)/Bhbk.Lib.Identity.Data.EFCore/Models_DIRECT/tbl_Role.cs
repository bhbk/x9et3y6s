using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Role
    {
        public tbl_Role()
        {
            tbl_AudienceRole = new HashSet<tbl_AudienceRole>();
            tbl_RoleClaim = new HashSet<tbl_RoleClaim>();
            tbl_UserRole = new HashSet<tbl_UserRole>();
        }

        public Guid Id { get; set; }
        public Guid AudienceId { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_Audience Audience { get; set; }
        public virtual ICollection<tbl_AudienceRole> tbl_AudienceRole { get; set; }
        public virtual ICollection<tbl_RoleClaim> tbl_RoleClaim { get; set; }
        public virtual ICollection<tbl_UserRole> tbl_UserRole { get; set; }
    }
}
