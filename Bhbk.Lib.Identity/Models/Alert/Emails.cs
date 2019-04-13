using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Alert
{
    public abstract class Emails
    {
        public Guid ActorId { get; set; }

        [Required]
        public Guid FromId { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string FromEmail { get; set; }

        public string FromDisplay { get; set; }

        public Guid ToId { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string ToEmail { get; set; }

        public string ToDisplay { get; set; }

        [Required]
        public string Subject { get; set; }

        public string HtmlContent { get; set; }

        public string PlaintextContent { get; set; }

        public DateTime Created { get; set; }

        [DefaultValue(typeof(DateTime))]
        public DateTime SendAt { get; set; }
    }

    public class EmailCreate : Emails
    {
        public Guid Id { get; set; }
    }
}
