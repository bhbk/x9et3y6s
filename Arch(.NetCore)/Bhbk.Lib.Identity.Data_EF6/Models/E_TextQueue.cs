//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bhbk.Lib.Identity.Data_EF6.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class E_TextQueue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public E_TextQueue()
        {
            this.TextActivity = new HashSet<E_TextActivity>();
        }
    
        public System.Guid Id { get; set; }
        public string FromPhoneNumber { get; set; }
        public string ToPhoneNumber { get; set; }
        public string Body { get; set; }
        public bool IsCancelled { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public System.DateTimeOffset SendAtUtc { get; set; }
        public Nullable<System.DateTimeOffset> DeliveredUtc { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<E_TextActivity> TextActivity { get; set; }
    }
}
