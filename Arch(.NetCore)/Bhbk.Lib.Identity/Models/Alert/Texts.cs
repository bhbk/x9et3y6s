using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Alert
{
    public abstract class Texts
    {
        public Guid? ActorId { get; set; }

        public Guid? FromId { get; set; }

        [DataType(DataType.PhoneNumber)]
        //[RegularExpression("^\\+[1-9]\\d{1,14}$")]
        public string FromPhoneNumber { get; set; }

        public Guid? ToId { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        //[RegularExpression("^\\+[1-9]\\d{1,14}$")]
        public string ToPhoneNumber { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public DateTimeOffset CreatedUtc { get; set; }

        [Required]
        public DateTimeOffset SendAtUtc { get; set; }
    }

    public class TextV1 : Texts
    {
        public Guid Id { get; set; }
    }
}
