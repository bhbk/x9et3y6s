using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Users
    {
        public Guid? ActorId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public bool IsHumanBeing { get; set; }

        public bool IsLockedOut { get; set; }

        [Required]
        public bool IsDeletable { get; set; }

        public string ConcurrencyStamp { get; set; }

        public string SecurityStamp { get; set; }

        public DateTimeOffset CreatedUtc { get; set; }
    }

    public class UserV1 : Users
    {
        [Required]
        public Guid Id { get; set; }

        public Guid IssuerId { get; set; }

        public bool EmailConfirmed { get; set; }

        public Nullable<bool> PhoneNumberConfirmed { get; set; }

        public Nullable<DateTimeOffset> LastUpdatedUtc { get; set; }

        public Nullable<DateTimeOffset> LockoutEndUtc { get; set; }

        public Nullable<DateTimeOffset> LastLoginFailureUtc { get; set; }

        public Nullable<DateTimeOffset> LastLoginSuccessUtc { get; set; }

        public int AccessFailedCount { get; set; }

        public int AccessSuccessCount { get; set; }

        public bool PasswordConfirmed { get; set; }

        public bool IsMultiFactor { get; set; }
    }
}
