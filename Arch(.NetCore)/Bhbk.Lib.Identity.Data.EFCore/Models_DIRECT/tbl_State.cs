using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_State
    {
        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? AudienceId { get; set; }
        public Guid? UserId { get; set; }
        public string StateValue { get; set; }
        public string StateType { get; set; }
        public bool? StateDecision { get; set; }
        public bool StateConsume { get; set; }
        public DateTimeOffset ValidFromUtc { get; set; }
        public DateTimeOffset ValidToUtc { get; set; }
        public DateTimeOffset IssuedUtc { get; set; }
        public DateTimeOffset LastPollingUtc { get; set; }

        public virtual tbl_Audience Audience { get; set; }
        public virtual tbl_Issuer Issuer { get; set; }
        public virtual tbl_User User { get; set; }
    }
}
