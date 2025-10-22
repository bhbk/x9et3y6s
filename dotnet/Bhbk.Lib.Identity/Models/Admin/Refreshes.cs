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
        public DateTimeOffset ValidFrom { get; set; }

        [Required]
        public DateTimeOffset ValidTo { get; set; }

        [Required]
        public DateTimeOffset Issued { get; set; }

        public string IpAddress { get; set; }

        public string UserAgent { get; set; }

        public string DeviceName { get; set; }

        public string Location { get; set; }
    }

    public class RefreshV1 : Refreshes
    {
        [Required]
        public Guid Id { get; set; }
    }
}
