using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppAudienceUri
    {
        public Guid Id { get; set; }
        public Guid AudienceId { get; set; }
        public Guid ActorId { get; set; }
        public string AbsoluteUri { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual AppUser Actor { get; set; }
        public virtual AppAudience Audience { get; set; }
    }
}
