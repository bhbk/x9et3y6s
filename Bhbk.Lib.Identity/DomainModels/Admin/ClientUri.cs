using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
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

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class ClientUriCreate : ClientUriBase
    {

    }

    public class ClientUriModel : ClientUriBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
