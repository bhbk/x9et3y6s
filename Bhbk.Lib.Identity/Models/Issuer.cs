using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
{
    public abstract class IssuerBase
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string IssuerKey { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class IssuerCreate : IssuerBase
    {
        public Guid ActorId { get; set; }
    }

    public class IssuerResult : IssuerBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }

        public IList<string> Clients { get; set; }
    }

    public class IssuerUpdate : IssuerBase
    {

        [Required]
        public Guid Id { get; set; }

        public Guid ActorId { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
