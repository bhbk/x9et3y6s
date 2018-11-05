using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Models
{
    public class ActivityCreate
    {
        public EntityEntry Entry { get; }
        public Guid ActorId { get; set; }
        public string ActivityType { get; set; }
        public string TableName { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> OriginalValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> CurrentValues { get; } = new Dictionary<string, object>();
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();
        public bool HasTemporaryProperties => TemporaryProperties.Any();
        public bool Immutable { get; set; }

        public ActivityCreate(EntityEntry entry)
        {
            Entry = entry;
        }
    }

    public class ActivityResult
    {
        public Guid Id { get; set; }
        public Guid ActorId { get; set; }
        public string ActivityType { get; set; }
        public string TableName { get; set; }
        public string KeyValues { get; set; }
        public string OriginalValues { get; set; }
        public string CurrentValues { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }
    }
}
