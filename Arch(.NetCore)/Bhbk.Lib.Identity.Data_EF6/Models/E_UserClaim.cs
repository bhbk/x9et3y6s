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
    
    public partial class E_UserClaim
    {
        public System.Guid UserId { get; set; }
        public System.Guid ClaimId { get; set; }
        public bool IsDeletable { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
    
        public virtual E_Claim Claim { get; set; }
        public virtual E_User User { get; set; }
    }
}
