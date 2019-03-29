using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppActivity
    {
        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string ActivityType { get; set; }
        public string TableName { get; set; }
        public string KeyValues { get; set; }
        public string OriginalValues { get; set; }
        public string CurrentValues { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual AppUser Actor { get; set; }
    }
}
