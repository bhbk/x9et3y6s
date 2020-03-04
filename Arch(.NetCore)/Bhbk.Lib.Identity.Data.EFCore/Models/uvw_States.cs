using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class uvw_States
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
        public DateTime IssuedUtc { get; set; }
        public DateTime LastPolling { get; set; }
    }
}
