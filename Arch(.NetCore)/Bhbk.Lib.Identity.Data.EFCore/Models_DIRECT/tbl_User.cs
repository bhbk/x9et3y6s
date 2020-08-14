using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_User
    {
        public tbl_User()
        {
            tbl_Activity = new HashSet<tbl_Activity>();
            tbl_Claim = new HashSet<tbl_Claim>();
            tbl_Login = new HashSet<tbl_Login>();
            tbl_QueueEmail = new HashSet<tbl_QueueEmail>();
            tbl_QueueText = new HashSet<tbl_QueueText>();
            tbl_Refresh = new HashSet<tbl_Refresh>();
            tbl_Setting = new HashSet<tbl_Setting>();
            tbl_State = new HashSet<tbl_State>();
            tbl_UserClaim = new HashSet<tbl_UserClaim>();
            tbl_UserLogin = new HashSet<tbl_UserLogin>();
            tbl_UserRole = new HashSet<tbl_UserRole>();
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
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime? LastLoginSuccess { get; set; }
        public DateTime? LastLoginFailure { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PasswordHashPBKDF2 { get; set; }
        public string PasswordHashSHA256 { get; set; }
        public bool PasswordConfirmed { get; set; }
        public string SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool HumanBeing { get; set; }
        public bool Immutable { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual ICollection<tbl_Activity> tbl_Activity { get; set; }
        public virtual ICollection<tbl_Claim> tbl_Claim { get; set; }
        public virtual ICollection<tbl_Login> tbl_Login { get; set; }
        public virtual ICollection<tbl_QueueEmail> tbl_QueueEmail { get; set; }
        public virtual ICollection<tbl_QueueText> tbl_QueueText { get; set; }
        public virtual ICollection<tbl_Refresh> tbl_Refresh { get; set; }
        public virtual ICollection<tbl_Setting> tbl_Setting { get; set; }
        public virtual ICollection<tbl_State> tbl_State { get; set; }
        public virtual ICollection<tbl_UserClaim> tbl_UserClaim { get; set; }
        public virtual ICollection<tbl_UserLogin> tbl_UserLogin { get; set; }
        public virtual ICollection<tbl_UserRole> tbl_UserRole { get; set; }
    }
}
