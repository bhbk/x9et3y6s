using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
{
    public abstract class ClientUriBase
    {
        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public Guid ActorId { get; set; }

        [Required]
        public string AbsoluteUri { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; }
    }

    public class ClientUriCreate : ClientUriBase { }

    public class ClientUriResult : ClientUriBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }

    public class ClientUriUpdate : ClientUriBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
