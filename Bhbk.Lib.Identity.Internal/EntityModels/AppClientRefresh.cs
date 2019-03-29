using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppClientRefresh
    {
        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid ClientId { get; set; }
        public string ProtectedTicket { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }

        public virtual AppClient Client { get; set; }
        public virtual AppIssuer Issuer { get; set; }
    }
}
