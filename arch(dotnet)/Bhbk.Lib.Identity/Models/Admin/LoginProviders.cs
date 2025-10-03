using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class LoginProviders
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public string ProviderKey { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public bool IsDeletable { get; set; }
    }

    public class LoginProviderV1 : LoginProviders
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreatedUtc { get; set; }

        public virtual ICollection<UserV1> Users { get; set; }
    }
}
