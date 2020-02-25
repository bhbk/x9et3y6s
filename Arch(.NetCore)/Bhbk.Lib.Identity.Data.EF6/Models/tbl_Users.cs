//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bhbk.Lib.Identity.Data.EF6.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_Users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Users()
        {
            this.tbl_Activities = new HashSet<tbl_Activities>();
            this.tbl_Claims = new HashSet<tbl_Claims>();
            this.tbl_Logins = new HashSet<tbl_Logins>();
            this.tbl_QueueEmails = new HashSet<tbl_QueueEmails>();
            this.tbl_QueueTexts = new HashSet<tbl_QueueTexts>();
            this.tbl_Refreshes = new HashSet<tbl_Refreshes>();
            this.tbl_Settings = new HashSet<tbl_Settings>();
            this.tbl_States = new HashSet<tbl_States>();
        }
    
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> ActorId { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public Nullable<bool> PhoneNumberConfirmed { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public bool LockoutEnabled { get; set; }
        public Nullable<System.DateTimeOffset> LockoutEnd { get; set; }
        public Nullable<System.DateTime> LastLoginSuccess { get; set; }
        public Nullable<System.DateTime> LastLoginFailure { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PasswordHash { get; set; }
        public bool PasswordConfirmed { get; set; }
        public string SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool HumanBeing { get; set; }
        public bool Immutable { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Activities> tbl_Activities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Claims> tbl_Claims { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Logins> tbl_Logins { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_QueueEmails> tbl_QueueEmails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_QueueTexts> tbl_QueueTexts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Refreshes> tbl_Refreshes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Settings> tbl_Settings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_States> tbl_States { get; set; }
    }
}
