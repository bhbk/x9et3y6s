using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_EntitlementScope
    {
        public tbl_EntitlementScope()
        {
            tbl_UserEntitlements = new HashSet<tbl_UserEntitlement>();
            tbl_AudienceEntitlements = new HashSet<tbl_AudienceEntitlement>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual ICollection<tbl_UserEntitlement> tbl_UserEntitlements { get; set; }
        public virtual ICollection<tbl_AudienceEntitlement> tbl_AudienceEntitlements { get; set; }
    }
}
