using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Me
{
    public abstract class Emails
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string NewEmail { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string NewEmailConfirm { get; set; }
    }

    public class UserAddEmail : Emails
    {

    }

    public class UserChangeEmail : Emails
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string CurrentEmail { get; set; }
    }
}
