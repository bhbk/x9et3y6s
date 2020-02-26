using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Alert
{
    public abstract class Texts
    {
        public Guid ActorId { get; set; }

        [Required]
        public Guid FromId { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string FromPhoneNumber { get; set; }

        public Guid ToId { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string ToPhoneNumber { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public DateTime SendAt { get; set; }
    }

    public class TextV1 : Texts
    {
        public Guid Id { get; set; }
    }
}
