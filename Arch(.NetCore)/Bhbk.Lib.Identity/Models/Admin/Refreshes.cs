using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Refreshes
    {
        [Required]
        public Guid IssuerId { get; set; }

        public Guid? AudienceId { get; set; }

        public Guid? UserId { get; set; }

        public string RefreshValue { get; set; }

        [Required]
        public string RefreshType { get; set; }

        [Required]
        public DateTimeOffset ValidFromUtc { get; set; }

        [Required]
        public DateTimeOffset ValidToUtc { get; set; }

        [Required]
        public DateTimeOffset IssuedUtc { get; set; }
    }

    public class RefreshV1 : Refreshes
    {
        [Required]
        public Guid Id { get; set; }
    }
}
