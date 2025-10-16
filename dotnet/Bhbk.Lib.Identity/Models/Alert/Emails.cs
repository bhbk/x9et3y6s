using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Alert
{
    public abstract class Emails
    {
        [DataType(DataType.EmailAddress)]
        public string FromEmail { get; set; }

        public string FromDisplay { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string ToEmail { get; set; }

        public string ToDisplay { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public bool IsCancelled { get; set; }

        [Required]
        public DateTimeOffset CreatedUtc { get; set; }

        [Required]
        public DateTimeOffset SendAtUtc { get; set; }
        public Nullable<DateTimeOffset> DeliveredUtc { get; set; }
    }

    public class EmailV1 : Emails
    {
        public Guid Id { get; set; }
    }
}
