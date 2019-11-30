using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class States
    {
        [Required]
        public Guid IssuerId { get; set; }

        public Guid? AudienceId { get; set; }

        public Guid? UserId { get; set; }

        public string StateValue { get; set; }

        [Required]
        public string StateType { get; set; }

        [Required]
        public bool? StateDecision { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool StateConsume { get; set; }

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
