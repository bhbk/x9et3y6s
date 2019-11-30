using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class tbl_States
    {
        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? AudienceId { get; set; }
        public Guid? UserId { get; set; }
        public string StateValue { get; set; }
        public string StateType { get; set; }
        public bool? StateDecision { get; set; }
        public bool StateConsume { get; set; }
        public DateTime ValidFromUtc { get; set; }
        public DateTime ValidToUtc { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastPolling { get; set; }

        public virtual tbl_Audiences Audience { get; set; }
        public virtual tbl_Issuers Issuer { get; set; }
        public virtual tbl_Users User { get; set; }
    }
}
