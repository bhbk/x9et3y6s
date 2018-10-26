using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identityuser?view=aspnetcore-1.1
    public partial class AppUser : IdentityUser<Guid>
    {

    }

    public abstract class UserBase
    {
        public Guid ClientId { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Required]
        [RegularExpression(@"^[\w\s-`']+$", ErrorMessage = "Special characters are not allowed (except - and `).")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[\w\s-`']+$", ErrorMessage = "Special characters are not allowed (except - and `).")]
        public string LastName { get; set; }

        public DateTime Created { get; set; }

        public bool LockoutEnabled { get; set; }

        [Required]
        public bool HumanBeing { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class UserCreate : UserBase
    {
        public Guid ActorId { get; set; }
    }

    public class UserResult : UserBase
    {
        public Guid Id { get; set; }

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

        public IList<string> Claims { get; set; }

        public IList<string> Roles { get; set; }

        public IList<string> Logins { get; set; }
    }

    public class UserUpdate : UserBase
    {
        [Required]
        public Guid Id { get; set; }

        public Guid ActorId { get; set; }

        [Required]
        public Nullable<DateTimeOffset> LockoutEnd { get; set; }
    }

    public class UserAddPassword
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string NewPasswordConfirm { get; set; }
    }

    public class UserAddPhoneNumber
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string NewPhoneNumber { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string NewPhoneNumberConfirm { get; set; }
    }

    public class UserChangePassword
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string NewPasswordConfirm { get; set; }
    }

    public class UserChangePhone
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string CurrentPhoneNumber { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string NewPhoneNumber { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string NewPhoneNumberConfirm { get; set; }
    }

    public class UserChangeEmail
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string CurrentEmail { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string NewEmail { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string NewEmailConfirm { get; set; }
    }

    public class UserQuoteOfDay
    {
        public Success success { get; set; }
        public Contents contents { get; set; }

        public class Contents
        {
            public List<Quote> quotes { get; set; }
        }

        public class Quote
        {
            public string quote { get; set; }
            public string length { get; set; }
            public string author { get; set; }
            public List<string> tags { get; set; }
            public string category { get; set; }
            public string date { get; set; }
            public string title { get; set; }
            public string background { get; set; }
            public string id { get; set; }
        }

        public class Success
        {
            public int total { get; set; }
        }
    }
}
