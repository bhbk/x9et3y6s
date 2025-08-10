using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_AuthActivity
    {
        public tbl_AuthActivity()
        {
            tbl_AuthActivityAudiences = new HashSet<tbl_AuthActivityAudience>();
        }

        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string LoginType { get; set; }
        public string LoginOutcome { get; set; }
        public string LocalEndpoint { get; set; }
        public string RemoteEndpoint { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }

        public virtual tbl_User User { get; set; }
        public virtual ICollection<tbl_AuthActivityAudience> tbl_AuthActivityAudiences { get; set; }
    }
}
