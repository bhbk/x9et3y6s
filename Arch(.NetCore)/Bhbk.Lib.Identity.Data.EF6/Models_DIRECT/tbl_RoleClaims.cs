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
    
    public partial class tbl_RoleClaims
    {
        public System.Guid RoleId { get; set; }
        public System.Guid ClaimId { get; set; }
        public Nullable<System.Guid> ActorId { get; set; }
        public System.DateTime Created { get; set; }
        public bool Immutable { get; set; }
    
        public virtual tbl_Claims tbl_Claims { get; set; }
        public virtual tbl_Roles tbl_Roles { get; set; }
    }
}
