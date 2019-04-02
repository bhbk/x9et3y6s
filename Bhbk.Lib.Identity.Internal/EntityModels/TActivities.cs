using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TActivities
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ClientId { get; set; }
        public string ActivityType { get; set; }
        public string TableName { get; set; }
        public string KeyValues { get; set; }
        public string OriginalValues { get; set; }
        public string CurrentValues { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual TClients Client { get; set; }
        public virtual TUsers User { get; set; }
    }
}
