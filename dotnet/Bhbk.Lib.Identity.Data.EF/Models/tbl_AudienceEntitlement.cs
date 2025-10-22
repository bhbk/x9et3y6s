using System;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_AudienceEntitlement
    {
        public Guid Id { get; set; }
        public Guid AudienceId { get; set; }
        public Guid EntitlementTypeId { get; set; }
        public Guid EntitlementScopeId { get; set; }
        public Guid? IssuerId { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual tbl_Audience Audience { get; set; }
        public virtual tbl_EntitlementType EntitlementType { get; set; }
        public virtual tbl_EntitlementScope EntitlementScope { get; set; }
        public virtual tbl_Issuer Issuer { get; set; }
    }
}
