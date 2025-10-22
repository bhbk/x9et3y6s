using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_User
    {
        public tbl_User()
        {
            tbl_UserAuthActivities = new HashSet<tbl_UserAuthActivity>();
            tbl_ChatConversations = new HashSet<tbl_ChatConversation>();
            tbl_ChatFavorites = new HashSet<tbl_ChatFavorite>();
            tbl_ChatPromptHistories = new HashSet<tbl_ChatPromptHistory>();
            tbl_UserEntitlements = new HashSet<tbl_UserEntitlement>();
            tbl_Refreshes = new HashSet<tbl_Refresh>();
            tbl_Settings = new HashSet<tbl_Setting>();
            tbl_States = new HashSet<tbl_State>();
            tbl_UserClaims = new HashSet<tbl_UserClaim>();
            tbl_UserLoginProviders = new HashSet<tbl_UserLoginProvider>();
            tbl_UserRoles = new HashSet<tbl_UserRole>();
        }

        public Guid Id { get; set; }
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
        public bool IsLockedOut { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual ICollection<tbl_UserAuthActivity> tbl_UserAuthActivities { get; set; }
        public virtual ICollection<tbl_ChatConversation> tbl_ChatConversations { get; set; }
        public virtual ICollection<tbl_ChatFavorite> tbl_ChatFavorites { get; set; }
        public virtual ICollection<tbl_ChatPromptHistory> tbl_ChatPromptHistories { get; set; }
        public virtual ICollection<tbl_UserEntitlement> tbl_UserEntitlements { get; set; }
        public virtual ICollection<tbl_Refresh> tbl_Refreshes { get; set; }
        public virtual ICollection<tbl_Setting> tbl_Settings { get; set; }
        public virtual ICollection<tbl_State> tbl_States { get; set; }
        public virtual ICollection<tbl_UserClaim> tbl_UserClaims { get; set; }
        public virtual ICollection<tbl_UserLoginProvider> tbl_UserLoginProviders { get; set; }
        public virtual ICollection<tbl_UserRole> tbl_UserRoles { get; set; }
    }
}
