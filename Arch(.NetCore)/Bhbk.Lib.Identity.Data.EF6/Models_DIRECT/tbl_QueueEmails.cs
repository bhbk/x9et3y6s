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
    
    public partial class tbl_QueueEmails
    {
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> ActorId { get; set; }
        public System.Guid FromId { get; set; }
        public string FromEmail { get; set; }
        public string FromDisplay { get; set; }
        public System.Guid ToId { get; set; }
        public string ToEmail { get; set; }
        public string ToDisplay { get; set; }
        public string Subject { get; set; }
        public string HtmlContent { get; set; }
        public string PlaintextContent { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime SendAt { get; set; }
    
        public virtual tbl_Users tbl_Users { get; set; }
    }
}