using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TUrls
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid? ActorId { get; set; }
        public string UrlHost { get; set; }
        public string UrlPath { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual TClients Client { get; set; }
    }
}
