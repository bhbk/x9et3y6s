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
    
    public partial class usp_Refresh_Delete_Result
    {
        public System.Guid Id { get; set; }
        public System.Guid IssuerId { get; set; }
        public Nullable<System.Guid> AudienceId { get; set; }
        public Nullable<System.Guid> UserId { get; set; }
        public string RefreshValue { get; set; }
        public string RefreshType { get; set; }
        public System.DateTimeOffset ValidFromUtc { get; set; }
        public System.DateTimeOffset ValidToUtc { get; set; }
        public System.DateTimeOffset IssuedUtc { get; set; }
    }
}
