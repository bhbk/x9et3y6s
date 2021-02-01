using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class uvw_AuthActivity
    {
        public Guid Id { get; set; }
        public Guid? AudienceId { get; set; }
        public Guid? UserId { get; set; }
        public string LoginType { get; set; }
        public string LoginOutcome { get; set; }
        public string LocalEndpoint { get; set; }
        public string RemoteEndpoint { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
