﻿using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppAudience
    {
        public AppAudience()
        {
            AppAudienceUri = new HashSet<AppAudienceUri>();
            AppRole = new HashSet<AppRole>();
        }

        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AudienceType { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual AppClient Client { get; set; }
        public virtual ICollection<AppAudienceUri> AppAudienceUri { get; set; }
        public virtual ICollection<AppRole> AppRole { get; set; }
    }
}
