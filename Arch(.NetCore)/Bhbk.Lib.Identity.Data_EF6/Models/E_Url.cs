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
    
    public partial class E_Url
    {
        public System.Guid Id { get; set; }
        public System.Guid AudienceId { get; set; }
        public string UrlHost { get; set; }
        public string UrlPath { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
    
        public virtual E_Audience Audience { get; set; }
    }
}
