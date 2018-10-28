using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppAudienceUri
    {

    }

    public abstract class AudienceUriBase
    {
        [Required]
        public Guid AudienceId { get; set; }

        [Required]
        public Guid ActorId { get; set; }

        [Required]
        public string AbsoluteUri { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; }
    }

    public class AudienceUriCreate : AudienceUriBase { }

    public class AudienceUriResult : AudienceUriBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }

    public class AudienceUriUpdate : AudienceUriBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
