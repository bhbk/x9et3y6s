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
    
    public partial class tbl_UserLogin
    {
        public System.Guid UserId { get; set; }
        public System.Guid LoginId { get; set; }
        public bool IsDeletable { get; set; }
        public Nullable<System.DateTimeOffset> CreatedUtc { get; set; }
        public System.DateTime VersionStartUtc { get; set; }
        public System.DateTime VersionEndUtc { get; set; }
    
        public virtual tbl_Login tbl_Login { get; set; }
        public virtual tbl_User tbl_User { get; set; }
    }
}
