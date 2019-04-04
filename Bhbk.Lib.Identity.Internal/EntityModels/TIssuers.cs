using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TIssuers
    {
        public TIssuers()
        {
            TClaims = new HashSet<TClaims>();
            TClients = new HashSet<TClients>();
            TRefreshes = new HashSet<TRefreshes>();
            TStates = new HashSet<TStates>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IssuerKey { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual ICollection<TClaims> TClaims { get; set; }
        public virtual ICollection<TClients> TClients { get; set; }
        public virtual ICollection<TRefreshes> TRefreshes { get; set; }
        public virtual ICollection<TStates> TStates { get; set; }
    }
}
