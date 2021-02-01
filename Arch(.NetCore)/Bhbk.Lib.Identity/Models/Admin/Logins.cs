using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Logins
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public string LoginKey { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public bool IsDeletable { get; set; }
    }

    public class LoginV1 : Logins
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
