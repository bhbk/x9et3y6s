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
        public string LoginProvider { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class LoginCreate : LoginBase
    {

    }

    public class LoginModel : LoginBase
    {
        [Required]
        public Guid Id { get; set; }

        public ICollection<string> Users { get; set; }
    }

    public class LoginUpdate : LoginBase
    {
        [Required]
        public Guid Id { get; set; }
    }
}
