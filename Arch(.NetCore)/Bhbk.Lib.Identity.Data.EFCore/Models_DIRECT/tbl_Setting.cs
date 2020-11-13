using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Setting
    {
        public Guid Id { get; set; }
        public Guid? IssuerId { get; set; }
        public Guid? AudienceId { get; set; }
        public Guid? UserId { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }

        public virtual tbl_Audience Audience { get; set; }
        public virtual tbl_Issuer Issuer { get; set; }
        public virtual tbl_User User { get; set; }
    }
}
