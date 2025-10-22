using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_UserAuthActivity
    {
        public tbl_UserAuthActivity()
        {
            tbl_AudienceAuthActivities = new HashSet<tbl_AudienceAuthActivity>();
        }

        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string LoginType { get; set; }
        public string LoginOutcome { get; set; }
        public string LocalEndpoint { get; set; }
        public string RemoteEndpoint { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual tbl_User User { get; set; }
        public virtual ICollection<tbl_AudienceAuthActivity> tbl_AudienceAuthActivities { get; set; }
    }
}
