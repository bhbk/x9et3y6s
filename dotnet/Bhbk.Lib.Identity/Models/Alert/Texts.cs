using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Alert
{
    public abstract class Texts
    {
        [DataType(DataType.PhoneNumber)]
        public string FromPhoneNumber { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string ToPhoneNumber { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public bool IsCancelled { get; set; }

        [Required]
        public DateTimeOffset Created { get; set; }

        [Required]
        public DateTimeOffset SendAt { get; set; }
        public Nullable<DateTimeOffset> Delivered { get; set; }
    }

    public class TextV1 : Texts
    {
        public Guid Id { get; set; }
    }
}
