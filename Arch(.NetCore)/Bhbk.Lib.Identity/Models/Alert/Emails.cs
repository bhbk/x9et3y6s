using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Alert
{
    public abstract class Emails
    {
        public Guid? ActorId { get; set; }

        public Guid? FromId { get; set; }

        [DataType(DataType.EmailAddress)]
        public string FromEmail { get; set; }

        public string FromDisplay { get; set; }

        public Guid? ToId { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string ToEmail { get; set; }

        public string ToDisplay { get; set; }

        [Required]
        public string Subject { get; set; }

        public string Body { get; set; }

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
