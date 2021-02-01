using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models_Tbl
{
    public partial class tbl_Refresh
    {
        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? AudienceId { get; set; }
        public Guid? UserId { get; set; }
        public string RefreshValue { get; set; }
        public string RefreshType { get; set; }
        public DateTimeOffset ValidFromUtc { get; set; }
        public DateTimeOffset ValidToUtc { get; set; }
        public DateTimeOffset IssuedUtc { get; set; }

        public virtual tbl_Audience Audience { get; set; }
        public virtual tbl_Issuer Issuer { get; set; }
        public virtual tbl_User User { get; set; }
    }
}
