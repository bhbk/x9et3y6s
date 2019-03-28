using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
{
    public abstract class ClaimBase
    {
        public Guid ActorId { get; set; }

        public Guid IssuerId { get; set; }

        public string Subject { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Value { get; set; }

        public string ValueType { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class ClaimCreate : ClaimBase
    {

    }

    public class ClaimModel : ClaimBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}