using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
{
    public abstract class States
    {
        [Required]
        public Guid IssuerId { get; set; }

        public Guid? ClientId { get; set; }

        public Guid? UserId { get; set; }

        public string NonceValue { get; set; }

        [Required]
        public string NonceType { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool NonceConsumed { get; set; }

        [Required]
        public DateTime ValidFromUtc { get; set; }

        [Required]
        public DateTime ValidToUtc { get; set; }
    }

    public class StateCreate : States
    {

    }

    public class StateModel : States
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }
    }
}
