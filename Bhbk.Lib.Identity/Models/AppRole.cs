﻿using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppRole
    {
        public AppRole()
        {
            AppRoleClaim = new HashSet<AppRoleClaim>();
            AppUserRole = new HashSet<AppUserRole>();
        }

        public Guid Id { get; set; }
        public Guid AudienceId { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string ConcurrencyStamp { get; set; }
        public bool Immutable { get; set; }

        public AppAudience Audience { get; set; }
        public ICollection<AppRoleClaim> AppRoleClaim { get; set; }
        public ICollection<AppUserRole> AppUserRole { get; set; }
    }
}