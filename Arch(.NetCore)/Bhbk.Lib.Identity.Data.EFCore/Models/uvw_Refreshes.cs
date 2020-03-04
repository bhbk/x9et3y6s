using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class uvw_Refreshes
    {
        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? AudienceId { get; set; }
        public Guid? UserId { get; set; }
        public string RefreshValue { get; set; }
        public string RefreshType { get; set; }
        public DateTime ValidFromUtc { get; set; }
        public DateTime ValidToUtc { get; set; }
        public DateTime IssuedUtc { get; set; }
    }
}
