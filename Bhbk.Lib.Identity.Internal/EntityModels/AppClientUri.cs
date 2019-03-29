using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppClientUri
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid? ActorId { get; set; }
        public string AbsoluteUri { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual AppClient Client { get; set; }
    }
}
