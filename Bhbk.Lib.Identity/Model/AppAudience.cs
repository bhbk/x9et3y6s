//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bhbk.Lib.Identity.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class AppAudience
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AppAudience()
        {
            this.Roles = new HashSet<AppRole>();
            this.Tokens = new HashSet<AppUserToken>();
        }
    
        public System.Guid Id { get; set; }
        public System.Guid ClientId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AudienceKey { get; set; }
        public string AudienceType { get; set; }
        public bool Enabled { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
    
        public virtual AppClient Clients { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AppRole> Roles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AppUserToken> Tokens { get; set; }
    }
}
