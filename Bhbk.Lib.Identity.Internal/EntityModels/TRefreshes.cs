using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TRefreshes
    {
        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? ClientId { get; set; }
        public Guid? UserId { get; set; }
        public string ProtectedTicket { get; set; }
        public string RefreshType { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }

        public virtual TClients Client { get; set; }
        public virtual TIssuers Issuer { get; set; }
        public virtual TUsers User { get; set; }
    }
}
