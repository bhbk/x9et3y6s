//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bhbk.Lib.Identity.Data_EF6.Models_Tbl
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_TextActivity
    {
        public System.Guid Id { get; set; }
        public System.Guid TextId { get; set; }
        public string TwilioSid { get; set; }
        public string TwilioStatus { get; set; }
        public string TwilioMessage { get; set; }
        public System.DateTimeOffset StatusAtUtc { get; set; }
    
        public virtual tbl_TextQueue tbl_TextQueue { get; set; }
    }
}
