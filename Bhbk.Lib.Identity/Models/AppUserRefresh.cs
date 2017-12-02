using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppUserRefresh
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid AudienceId { get; set; }
        public Guid UserId { get; set; }
        public string ProtectedTicket { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }

        public AppAudience Audience { get; set; }
        public AppClient Client { get; set; }
        public AppUser User { get; set; }
    }
}
