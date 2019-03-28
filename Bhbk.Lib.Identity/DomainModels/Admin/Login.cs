using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
{
    public abstract class LoginBase
    {
        public Guid ActorId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public string LoginKey { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }

        public ICollection<string> Users { get; set; }
    }

    public class LoginCreate : LoginBase
    {

    }

    public class LoginModel : LoginBase
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
