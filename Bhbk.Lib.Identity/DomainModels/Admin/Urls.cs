using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
{
    public abstract class Urls
    {
        public Guid ActorId { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public string UrlHost { get; set; }

        [Required]
        public string UrlPath { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class UrlCreate : Urls
    {

    }

    public class UrlModel : Urls
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
