using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Me
{
    public abstract class Phones
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string NewPhoneNumber { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string NewPhoneNumberConfirm { get; set; }
    }

    public class UserAddPhone : Phones
    {

    }

    public class UserChangePhone : Phones
    {
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string CurrentPhoneNumber { get; set; }
    }
}
