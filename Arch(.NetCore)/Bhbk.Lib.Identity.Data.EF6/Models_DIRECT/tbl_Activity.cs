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
    
    public partial class tbl_Activity
    {
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> AudienceId { get; set; }
        public Nullable<System.Guid> UserId { get; set; }
        public string ActivityType { get; set; }
        public string TableName { get; set; }
        public string KeyValues { get; set; }
        public string OriginalValues { get; set; }
        public string CurrentValues { get; set; }
        public bool IsDeletable { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
    
        public virtual tbl_Audience tbl_Audience { get; set; }
        public virtual tbl_User tbl_User { get; set; }
    }
}
