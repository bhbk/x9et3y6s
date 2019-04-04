using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TUsers
    {
        public TUsers()
        {
            TActivities = new HashSet<TActivities>();
            TClaims = new HashSet<TClaims>();
            TLogins = new HashSet<TLogins>();
            TRefreshes = new HashSet<TRefreshes>();
            TStates = new HashSet<TStates>();
            TUserClaims = new HashSet<TUserClaims>();
            TUserLogins = new HashSet<TUserLogins>();
            TUserRoles = new HashSet<TUserRoles>();
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

        public virtual ICollection<TActivities> TActivities { get; set; }
        public virtual ICollection<TClaims> TClaims { get; set; }
        public virtual ICollection<TLogins> TLogins { get; set; }
        public virtual ICollection<TRefreshes> TRefreshes { get; set; }
        public virtual ICollection<TStates> TStates { get; set; }
        public virtual ICollection<TUserClaims> TUserClaims { get; set; }
        public virtual ICollection<TUserLogins> TUserLogins { get; set; }
        public virtual ICollection<TUserRoles> TUserRoles { get; set; }
    }
}
