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
    
    public partial class tbl_TextQueue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_TextQueue()
        {
            this.tbl_TextActivity = new HashSet<tbl_TextActivity>();
        }
    
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> ActorId { get; set; }
        public Nullable<System.Guid> FromId { get; set; }
        public string FromPhoneNumber { get; set; }
        public System.Guid ToId { get; set; }
        public string ToPhoneNumber { get; set; }
        public string Body { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public System.DateTimeOffset SendAtUtc { get; set; }
        public Nullable<System.DateTimeOffset> DeliveredUtc { get; set; }
    
        public virtual tbl_User tbl_User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_TextActivity> tbl_TextActivity { get; set; }
    }
}
