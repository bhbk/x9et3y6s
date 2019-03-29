using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppUser
    {
        public AppUser()
        {
            AppActivity = new HashSet<AppActivity>();
            AppClaim = new HashSet<AppClaim>();
            AppLogin = new HashSet<AppLogin>();
            AppUserClaim = new HashSet<AppUserClaim>();
            AppUserCode = new HashSet<AppUserCode>();
            AppUserLogin = new HashSet<AppUserLogin>();
            AppUserRefresh = new HashSet<AppUserRefresh>();
            AppUserRole = new HashSet<AppUserRole>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime? LastLoginSuccess { get; set; }
        public DateTime? LastLoginFailure { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PasswordHash { get; set; }
        public bool PasswordConfirmed { get; set; }
        public string SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool HumanBeing { get; set; }
        public bool Immutable { get; set; }

        public virtual ICollection<AppActivity> AppActivity { get; set; }
        public virtual ICollection<AppClaim> AppClaim { get; set; }
        public virtual ICollection<AppLogin> AppLogin { get; set; }
        public virtual ICollection<AppUserClaim> AppUserClaim { get; set; }
        public virtual ICollection<AppUserCode> AppUserCode { get; set; }
        public virtual ICollection<AppUserLogin> AppUserLogin { get; set; }
        public virtual ICollection<AppUserRefresh> AppUserRefresh { get; set; }
        public virtual ICollection<AppUserRole> AppUserRole { get; set; }
    }
}
