//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bhbk.Lib.Identity.Data.EF6.Models_DIRECT
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_Audiences
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Audiences()
        {
            this.tbl_Activities = new HashSet<tbl_Activities>();
            this.tbl_AudienceRoles = new HashSet<tbl_AudienceRoles>();
            this.tbl_Refreshes = new HashSet<tbl_Refreshes>();
            this.tbl_Roles = new HashSet<tbl_Roles>();
            this.tbl_Settings = new HashSet<tbl_Settings>();
            this.tbl_States = new HashSet<tbl_States>();
            this.tbl_Urls = new HashSet<tbl_Urls>();
        }
    
        public System.Guid Id { get; set; }
        public System.Guid IssuerId { get; set; }
        public Nullable<System.Guid> ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PasswordHashPBKDF2 { get; set; }
        public string PasswordHashSHA256 { get; set; }
        public string SecurityStamp { get; set; }
        public bool Enabled { get; set; }
        public System.DateTime Created { get; set; }
        public bool LockoutEnabled { get; set; }
        public Nullable<System.DateTimeOffset> LockoutEnd { get; set; }
        public Nullable<System.DateTime> LastLoginSuccess { get; set; }
        public Nullable<System.DateTime> LastLoginFailure { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Activities> tbl_Activities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_AudienceRoles> tbl_AudienceRoles { get; set; }
        public virtual tbl_Issuers tbl_Issuers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Refreshes> tbl_Refreshes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Roles> tbl_Roles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Settings> tbl_Settings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_States> tbl_States { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Urls> tbl_Urls { get; set; }
    }
}
