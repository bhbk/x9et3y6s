using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class tbl_Users
    {
        public tbl_Users()
        {
            tbl_Activities = new HashSet<tbl_Activities>();
            tbl_Claims = new HashSet<tbl_Claims>();
            tbl_Logins = new HashSet<tbl_Logins>();
            tbl_QueueEmails = new HashSet<tbl_QueueEmails>();
            tbl_QueueTexts = new HashSet<tbl_QueueTexts>();
            tbl_Refreshes = new HashSet<tbl_Refreshes>();
            tbl_Settings = new HashSet<tbl_Settings>();
            tbl_States = new HashSet<tbl_States>();
            tbl_UserClaims = new HashSet<tbl_UserClaims>();
            tbl_UserLogins = new HashSet<tbl_UserLogins>();
            tbl_UserRoles = new HashSet<tbl_UserRoles>();
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

        public virtual ICollection<tbl_Activities> tbl_Activities { get; set; }
        public virtual ICollection<tbl_Claims> tbl_Claims { get; set; }
        public virtual ICollection<tbl_Logins> tbl_Logins { get; set; }
        public virtual ICollection<tbl_QueueEmails> tbl_QueueEmails { get; set; }
        public virtual ICollection<tbl_QueueTexts> tbl_QueueTexts { get; set; }
        public virtual ICollection<tbl_Refreshes> tbl_Refreshes { get; set; }
        public virtual ICollection<tbl_Settings> tbl_Settings { get; set; }
        public virtual ICollection<tbl_States> tbl_States { get; set; }
        public virtual ICollection<tbl_UserClaims> tbl_UserClaims { get; set; }
        public virtual ICollection<tbl_UserLogins> tbl_UserLogins { get; set; }
        public virtual ICollection<tbl_UserRoles> tbl_UserRoles { get; set; }
    }
}
