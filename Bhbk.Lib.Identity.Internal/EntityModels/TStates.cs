using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TStates
    {
        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? ClientId { get; set; }
        public Guid? UserId { get; set; }
        public string NonceValue { get; set; }
        public string NonceType { get; set; }
        public bool NonceConsumed { get; set; }
        public DateTime ValidFromUtc { get; set; }
        public DateTime ValidToUtc { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastPolling { get; set; }

        public virtual TClients Client { get; set; }
        public virtual TIssuers Issuer { get; set; }
        public virtual TUsers User { get; set; }
    }
}
