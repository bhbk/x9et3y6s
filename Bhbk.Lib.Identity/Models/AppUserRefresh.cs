using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppUserRefresh
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid UserId { get; set; }
        public string ProtectedTicket { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }

        public virtual AppClient Client { get; set; }
        public virtual AppUser User { get; set; }
    }
}
