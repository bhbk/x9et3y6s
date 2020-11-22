using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Urls
    {
        [Required]
        public Guid AudienceId { get; set; }

        [Required]
        public string UrlHost { get; set; }

        [Required]
        public string UrlPath { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public bool IsDeletable { get; set; }
    }

    public class UrlV1 : Urls
    {
        [Required]
        public Guid Id { get; set; }

        public DateTimeOffset CreatedUtc { get; set; }

        public Nullable<DateTimeOffset> LastUpdatedUtc { get; set; }
    }
}
