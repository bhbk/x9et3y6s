using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Users
    {
        public Guid? ActorId { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public DateTime Created { get; set; }

        public bool LockoutEnabled { get; set; }

        public string ConcurrencyStamp { get; set; }

        [Required]
        public bool HumanBeing { get; set; }

        [Required]
        public bool Immutable { get; set; }
    }

    public class UserV1 : Users
    {
        [Required]
        public Guid Id { get; set; }

        public Guid IssuerId { get; set; }

        public bool EmailConfirmed { get; set; }

        public Nullable<bool> PhoneNumberConfirmed { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }

        public Nullable<DateTimeOffset> LockoutEnd { get; set; }

        public Nullable<DateTime> LastLoginFailure { get; set; }

        public Nullable<DateTime> LastLoginSuccess { get; set; }

        public int AccessFailedCount { get; set; }

        public int AccessSuccessCount { get; set; }

        public bool PasswordConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }
    }
}
