using Bhbk.Lib.Identity.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Factory
{
    public class UserFactory<T> : AppUser
    {
        public UserFactory(AppUser user)
        {
            this.Id = user.Id;
            this.UserName = user.UserName;
            this.NormalizedUserName = user.NormalizedUserName ?? string.Empty;
            this.Email = user.Email;
            this.NormalizedEmail = user.NormalizedEmail;
            this.EmailConfirmed = user.EmailConfirmed;
            this.PhoneNumber = user.PhoneNumber ?? string.Empty;
            this.PhoneNumberConfirmed = user.PhoneNumberConfirmed.HasValue ? user.PhoneNumberConfirmed : false;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.Created = user.Created;
            this.LastUpdated = user.LastUpdated;
            this.LockoutEnabled = user.LockoutEnabled;
            this.LockoutEnd = user.LockoutEnd;
            this.LastLoginFailure = user.LastLoginFailure;
            this.LastLoginSuccess = user.LastLoginSuccess;
            this.AccessFailedCount = user.AccessFailedCount;
            this.AccessSuccessCount = user.AccessSuccessCount;
            this.ConcurrencyStamp = user.ConcurrencyStamp;
            this.PasswordHash = user.PasswordHash;
            this.PasswordConfirmed = user.PasswordConfirmed;
            this.SecurityStamp = user.SecurityStamp;
            this.TwoFactorEnabled = user.TwoFactorEnabled;
            this.Immutable = user.Immutable;
        }

        public UserFactory(UserCreate user)
        {
            this.Id = Guid.NewGuid();
            this.UserName = user.Email;
            this.NormalizedUserName = string.Empty;
            this.Email = user.Email;
            this.NormalizedEmail = string.Empty;
            this.EmailConfirmed = false;
            this.PhoneNumber = user.PhoneNumber;
            this.PhoneNumberConfirmed = false;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.Created = DateTime.Now;
            this.LockoutEnabled = false;
            this.AccessFailedCount = 0;
            this.AccessSuccessCount = 0;
            this.ConcurrencyStamp = string.Empty;
            this.PasswordHash = string.Empty;
            this.PasswordConfirmed = false;
            this.SecurityStamp = string.Empty;
            this.TwoFactorEnabled = false;
            this.Immutable = false;
        }

        public UserFactory(UserUpdate user)
        {
            this.Id = user.Id;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.LastUpdated = DateTime.Now;
            this.LockoutEnabled = user.LockoutEnabled;
            this.LockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.ToUniversalTime() : user.LockoutEnd;
        }

        public AppUser Devolve()
        {
            return new AppUser
            {
                Id = this.Id,
                UserName = this.UserName,
                NormalizedUserName = this.NormalizedUserName,
                Email = this.Email,
                NormalizedEmail = this.NormalizedEmail,
                EmailConfirmed = this.EmailConfirmed,
                PhoneNumber = this.PhoneNumber,
                PhoneNumberConfirmed = this.PhoneNumberConfirmed,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Created = this.Created,
                LastUpdated = this.LastUpdated,
                LockoutEnabled = this.LockoutEnabled,
                LockoutEnd = this.LockoutEnd,
                LastLoginFailure = this.LastLoginFailure,
                LastLoginSuccess = this.LastLoginSuccess,
                AccessFailedCount = this.AccessFailedCount,
                AccessSuccessCount = this.AccessSuccessCount,
                ConcurrencyStamp = this.ConcurrencyStamp,
                PasswordHash = this.PasswordHash,
                PasswordConfirmed = this.PasswordConfirmed,
                SecurityStamp = this.SecurityStamp,
                TwoFactorEnabled = this.TwoFactorEnabled,
                Immutable = this.Immutable,
            };
        }

        public UserResult Evolve()
        {
            return new UserResult
            {
                Id = this.Id,
                Email = this.Email,
                EmailConfirmed = this.EmailConfirmed,
                PhoneNumber = this.PhoneNumber,
                PhoneNumberConfirmed = this.PhoneNumberConfirmed,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Created = this.Created,
                LastUpdated = this.LastUpdated,
                LockoutEnabled = this.LockoutEnabled,
                LockoutEnd = this.LockoutEnd,
                LastLoginFailure = this.LastLoginFailure,
                LastLoginSuccess = this.LastLoginSuccess,
                AccessFailedCount = this.AccessFailedCount,
                AccessSuccessCount = this.AccessSuccessCount,
                PasswordConfirmed = this.PasswordConfirmed,
                TwoFactorEnabled = this.TwoFactorEnabled,
                Immutable = this.Immutable,
            };
        }
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

    public class UserResult
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
        public Nullable<DateTimeOffset> LockoutEnd { get; set; }
        public Nullable<DateTime> LastLoginFailure { get; set; }
        public Nullable<DateTime> LastLoginSuccess { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public bool PasswordConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool Immutable { get; set; }
        public IList<Claim> Claims { get; set; }
        public IList<RoleResult> Roles { get; set; }
    }

    public class UserUpdate
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool LockoutEnabled { get; set; }
        public Nullable<DateTimeOffset> LockoutEnd { get; set; }
    }

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
