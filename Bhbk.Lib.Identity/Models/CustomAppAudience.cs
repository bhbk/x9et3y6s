using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppAudience
    {

    }

    public abstract class AudienceBase
    {
        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string AudienceType { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class AudienceCreate : AudienceBase
    {
        public Guid ActorId { get; set; }
    }

    public class AudienceResult : AudienceBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }

        public IList<string> Roles { get; set; }
    }

    public class AudienceUpdate : AudienceBase
    {
        [Required]
        public Guid Id { get; set; }

        public Guid ActorId { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
