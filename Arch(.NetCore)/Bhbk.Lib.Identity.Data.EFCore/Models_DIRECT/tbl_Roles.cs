﻿using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Roles
    {
        public tbl_Roles()
        {
            tbl_AudienceRoles = new HashSet<tbl_AudienceRoles>();
            tbl_RoleClaims = new HashSet<tbl_RoleClaims>();
            tbl_UserRoles = new HashSet<tbl_UserRoles>();
        }

        public Guid Id { get; set; }
        public Guid AudienceId { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Audiences Audience { get; set; }
        public virtual ICollection<tbl_AudienceRoles> tbl_AudienceRoles { get; set; }
        public virtual ICollection<tbl_RoleClaims> tbl_RoleClaims { get; set; }
        public virtual ICollection<tbl_UserRoles> tbl_UserRoles { get; set; }
    }
}