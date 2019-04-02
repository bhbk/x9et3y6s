using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TClients
    {
        public TClients()
        {
            TActivities = new HashSet<TActivities>();
            TClientUrls = new HashSet<TClientUrls>();
            TCodes = new HashSet<TCodes>();
            TRefreshes = new HashSet<TRefreshes>();
            TRoles = new HashSet<TRoles>();
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
        public virtual ICollection<TClientUrls> TClientUrls { get; set; }
        public virtual ICollection<TCodes> TCodes { get; set; }
        public virtual ICollection<TRefreshes> TRefreshes { get; set; }
        public virtual ICollection<TRoles> TRoles { get; set; }
    }
}
