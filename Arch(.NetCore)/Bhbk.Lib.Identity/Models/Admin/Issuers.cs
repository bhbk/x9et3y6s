using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Issuers
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string IssuerKey { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public bool IsDeletable { get; set; }
    }

    public class IssuerV1 : Issuers
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreatedUtc { get; set; }

        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
