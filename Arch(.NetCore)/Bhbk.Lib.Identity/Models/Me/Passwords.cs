using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Me
{
    public abstract class Passwords
    {
        [Required]
        public Guid EntityId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPasswordConfirm { get; set; }
    }

    public class EntityAddPassword : Passwords
    {

    }

    public class EntityChangePassword : Passwords
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
    }
}
