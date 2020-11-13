using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_User
    {
        public tbl_User()
        {
            tbl_Activities = new HashSet<tbl_Activity>();
            tbl_Claims = new HashSet<tbl_Claim>();
            tbl_EmailQueues = new HashSet<tbl_EmailQueue>();
            tbl_Logins = new HashSet<tbl_Login>();
            tbl_Refreshes = new HashSet<tbl_Refresh>();
            tbl_Settings = new HashSet<tbl_Setting>();
            tbl_States = new HashSet<tbl_State>();
            tbl_TextQueues = new HashSet<tbl_TextQueue>();
            tbl_UserClaims = new HashSet<tbl_UserClaim>();
            tbl_UserLogins = new HashSet<tbl_UserLogin>();
            tbl_UserRoles = new HashSet<tbl_UserRole>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PasswordHashPBKDF2 { get; set; }
        public string PasswordHashSHA256 { get; set; }
        public bool PasswordConfirmed { get; set; }
        public string SecurityStamp { get; set; }
        public bool IsHumanBeing { get; set; }
        public bool IsMultiFactor { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsDeletable { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public DateTimeOffset? LockoutEndUtc { get; set; }
        public DateTimeOffset? LastLoginSuccessUtc { get; set; }
        public DateTimeOffset? LastLoginFailureUtc { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual ICollection<tbl_Activity> tbl_Activities { get; set; }
        public virtual ICollection<tbl_Claim> tbl_Claims { get; set; }
        public virtual ICollection<tbl_EmailQueue> tbl_EmailQueues { get; set; }
        public virtual ICollection<tbl_Login> tbl_Logins { get; set; }
        public virtual ICollection<tbl_Refresh> tbl_Refreshes { get; set; }
        public virtual ICollection<tbl_Setting> tbl_Settings { get; set; }
        public virtual ICollection<tbl_State> tbl_States { get; set; }
        public virtual ICollection<tbl_TextQueue> tbl_TextQueues { get; set; }
        public virtual ICollection<tbl_UserClaim> tbl_UserClaims { get; set; }
        public virtual ICollection<tbl_UserLogin> tbl_UserLogins { get; set; }
        public virtual ICollection<tbl_UserRole> tbl_UserRoles { get; set; }
    }
}
