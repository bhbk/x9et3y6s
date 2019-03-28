using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Alert
{
    public abstract class TextBase
    {
        public Guid Id { get; set; }

        [Required]
        public Guid FromId { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string FromPhoneNumber { get; set; }

        [Required]
        public Guid ToId { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string ToPhoneNumber { get; set; }

        [Required]
        public string Body { get; set; }

        public DateTime Created { get; set; }
    }

    public class TextCreate : TextBase
    {
        public Guid ActorId { get; set; }
    }
}
