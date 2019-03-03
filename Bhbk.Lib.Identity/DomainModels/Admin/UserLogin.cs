using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
{
    public abstract class UserLoginBase
    {
        public Guid ActorId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid LoginId { get; set; }

        [Required]
        public string LoginProvider { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ProviderDescription { get; set; }

        public string ProviderKey { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class UserLoginCreate : UserLoginBase
    {

    }

    public class UserLoginModel : UserLoginBase
    {
        [Required]
        public DateTime Created { get; set; }

        [Required]
        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
