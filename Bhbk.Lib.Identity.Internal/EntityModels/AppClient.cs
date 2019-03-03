using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppClient
    {
        public AppClient()
        {
            AppClientUri = new HashSet<AppClientUri>();
            AppRole = new HashSet<AppRole>();
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

        public virtual AppIssuer Issuer { get; set; }
        public virtual ICollection<AppClientUri> AppClientUri { get; set; }
        public virtual ICollection<AppRole> AppRole { get; set; }
    }
}
