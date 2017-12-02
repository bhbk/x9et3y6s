using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Factory
{
    public class UserRefreshCreate
    {
        public Guid ClientId { get; set; }
        public Guid AudienceId { get; set; }
        public Guid UserId { get; set; }
        public string ProtectedTicket { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
    }

    public class UserRefreshModel
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid AudienceId { get; set; }
        public Guid UserId { get; set; }
        public string ProtectedTicket { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
    }
}
