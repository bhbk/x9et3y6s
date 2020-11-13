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
    
    public partial class tbl_EmailQueue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_EmailQueue()
        {
            this.tbl_EmailActivity = new HashSet<tbl_EmailActivity>();
        }
    
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> ActorId { get; set; }
        public Nullable<System.Guid> FromId { get; set; }
        public string FromEmail { get; set; }
        public string FromDisplay { get; set; }
        public System.Guid ToId { get; set; }
        public string ToEmail { get; set; }
        public string ToDisplay { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public System.DateTimeOffset SendAtUtc { get; set; }
        public Nullable<System.DateTimeOffset> DeliveredUtc { get; set; }
    
        public virtual tbl_User tbl_User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_EmailActivity> tbl_EmailActivity { get; set; }
    }
}
