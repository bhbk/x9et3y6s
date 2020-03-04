using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Logins
    {
        public Guid? ActorId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public string LoginKey { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class LoginV1 : Logins
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
