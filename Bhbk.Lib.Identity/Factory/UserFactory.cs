using Bhbk.Lib.Identity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

//TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
namespace Bhbk.Lib.Identity.Factory
{
    public class UserFactory<T> : AppUser
    {
        public UserFactory(AppUser user)
        {
            this.Id = user.Id;
            this.ActorId = user.ActorId;
            this.UserName = user.UserName;
            this.NormalizedUserName = user.UserName;
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
            this.HumanBeing = user.HumanBeing;
            this.Immutable = user.Immutable;
            this.AppUserClaim = user.AppUserClaim;
            this.AppUserLogin = user.AppUserLogin;
            this.AppUserRole = user.AppUserRole;

            if (!user.HumanBeing)
                user.EmailConfirmed = true;
        }

        public UserFactory(UserCreate user)
        {
            this.Id = Guid.NewGuid();
            this.ActorId = user.ActorId;
            this.UserName = user.Email;
            this.NormalizedUserName = user.Email;
            this.Email = user.Email;
            this.NormalizedEmail = user.Email;
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
            this.HumanBeing = user.HumanBeing;
            this.Immutable = user.Immutable;
        }

        public AppUser Devolve()
        {
            return new AppUser
            {
                ActorId = this.ActorId,
                Id = this.Id,
                UserName = this.Email,
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
                HumanBeing = this.HumanBeing,
                Immutable = this.Immutable,
                AppUserClaim = this.AppUserClaim,
                AppUserLogin = this.AppUserLogin,
                AppUserRole = this.AppUserRole,
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
                HumanBeing = this.HumanBeing,
                Immutable = this.Immutable,
                Claims = AppUserClaim.Where(x => x.UserId == this.Id).Select(x => x.UserId.ToString()).ToList(),
                Logins = AppUserLogin.Where(x => x.UserId == this.Id).Select(x => x.LoginId.ToString()).ToList(),
                Roles = AppUserRole.Where(x => x.UserId == this.Id).Select(x => x.RoleId.ToString()).ToList(),
            };
        }

        public void Update(UserUpdate user)
        {
            this.Id = user.Id;
            this.ActorId = user.ActorId;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.LastUpdated = DateTime.Now;
            this.LockoutEnabled = user.LockoutEnabled;
            this.LockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.ToUniversalTime() : user.LockoutEnd;
        }
    }

    public class UserCreate
    {
        public Guid ActorId { get; set; }

        [Required]
        [EmailAddress]
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

    public class UserCreateEmail
    {
        public Guid Id { get; set; }

        [Required]
        public Guid FromId { get; set; }

        [Required]
        [EmailAddress]
        public string FromEmail { get; set; }

        public string FromDisplay { get; set; }

        [Required]
        public Guid ToId { get; set; }

        [Required]
        [EmailAddress]
        public string ToEmail { get; set; }

        public string ToDisplay { get; set; }

        [Required]
        public string Subject { get; set; }

        public string HtmlContent { get; set; }

        public string PLaintextContent { get; set; }

        public DateTime Created { get; set; }
    }

    public class UserCreateText
    {
        public Guid Id { get; set; }

        [Required]
        public Guid FromId { get; set; }

        [Required]
        [Phone]
        public string FromPhoneNumber { get; set; }

        [Required]
        public Guid ToId { get; set; }

        [Required]
        [Phone]
        public string ToPhoneNumber { get; set; }

        [Required]
        public string Body { get; set; }

        public DateTime Created { get; set; }
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
        public bool HumanBeing { get; set; }
        public bool Immutable { get; set; }
        public IList<string> Claims { get; set; }
        public IList<string> Roles { get; set; }
        public IList<string> Logins { get; set; }
    }

    public class UserUpdate
    {
        [Required]
        public Guid Id { get; set; }

        public Guid ActorId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public bool LockoutEnabled { get; set; }

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
        public string NewPhoneNumber { get; set; }

        [Required]
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
        public string CurrentPhoneNumber { get; set; }

        [Required]
        public string NewPhoneNumber { get; set; }

        [Required]
        public string NewPhoneNumberConfirm { get; set; }
    }

    public class UserChangeEmail
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string CurrentEmail { get; set; }

        [Required]
        public string NewEmail { get; set; }

        [Required]
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
