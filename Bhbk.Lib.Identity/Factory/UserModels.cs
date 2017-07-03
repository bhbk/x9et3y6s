using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Factory
{
    public class UserAddPassword
    {
        public Guid Id { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordConfirm { get; set; }
    }

    public class UserAddPhoneNumber
    {
        public Guid Id { get; set; }
        public string NewPhoneNumber { get; set; }
        public string NewPhoneNumberConfirm { get; set; }
    }

    public class UserChangePassword
    {
        public Guid Id { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordConfirm { get; set; }
    }

    public class UserChangePhone
    {
        public Guid Id { get; set; }
        public string CurrentPhoneNumber { get; set; }
        public string NewPhoneNumber { get; set; }
        public string NewPhoneNumberConfirm { get; set; }
    }

    public class UserChangeEmail
    {
        public Guid Id { get; set; }
        public string CurrentEmail { get; set; }
        public string NewEmail { get; set; }
        public string NewEmailConfirm { get; set; }
    }

    public class UserCreate
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Created { get; set; }
        public bool LockoutEnabled { get; set; }
        public bool Immutable { get; set; }
    }

    public class UserModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public Nullable<bool> PhoneNumberConfirmed { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool LockoutEnabled { get; set; }
        public Nullable<DateTime> LockoutEndDateUtc { get; set; }
        public Nullable<DateTime> LastLoginFailure { get; set; }
        public Nullable<DateTime> LastLoginSuccess { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public bool PasswordConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool Immutable { get; set; }
        public IList<UserClaimModel> Claims { get; set; }
        public IList<RoleModel> Roles { get; set; }
    }

    public class UserUpdate
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool LockoutEnabled { get; set; }
        public Nullable<DateTime> LockoutEndDateUtc { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool Immutable { get; set; }
    }
}
