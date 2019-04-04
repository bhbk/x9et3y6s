using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TClients
    {
        public TClients()
        {
            TActivities = new HashSet<TActivities>();
            TRefreshes = new HashSet<TRefreshes>();
            TRoles = new HashSet<TRoles>();
            TStates = new HashSet<TStates>();
            TUrls = new HashSet<TUrls>();
        }

        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClientKey { get; set; }
        public string ClientType { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual TIssuers Issuer { get; set; }
        public virtual ICollection<TActivities> TActivities { get; set; }
        public virtual ICollection<TRefreshes> TRefreshes { get; set; }
        public virtual ICollection<TRoles> TRoles { get; set; }
        public virtual ICollection<TStates> TStates { get; set; }
        public virtual ICollection<TUrls> TUrls { get; set; }
    }
}
