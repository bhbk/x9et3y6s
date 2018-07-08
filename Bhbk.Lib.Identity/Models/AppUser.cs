using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppUser
    {
        public AppUser()
        {
            AppAudienceUri = new HashSet<AppAudienceUri>();
            AppUserClaim = new HashSet<AppUserClaim>();
            AppUserLogin = new HashSet<AppUserLogin>();
            AppUserRefresh = new HashSet<AppUserRefresh>();
            AppUserRole = new HashSet<AppUserRole>();
            AppUserToken = new HashSet<AppUserToken>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string NormalizedUserName { get; set; }
        public string NormalizedEmail { get; set; }
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

        public ICollection<AppAudienceUri> AppAudienceUri { get; set; }
        public ICollection<AppUserClaim> AppUserClaim { get; set; }
        public ICollection<AppUserLogin> AppUserLogin { get; set; }
        public ICollection<AppUserRefresh> AppUserRefresh { get; set; }
        public ICollection<AppUserRole> AppUserRole { get; set; }
        public ICollection<AppUserToken> AppUserToken { get; set; }
    }
}
