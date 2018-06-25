using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Factory
{
    //TODO https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.1
    public class ActivityFactory<T> : AppActivity
    {
        public ActivityFactory(AppActivity activity)
        {
            this.Id = activity.Id;
            this.ActorId = activity.ActorId;
            this.ActivityType = activity.ActivityType;
            this.TableName = TableName;
            this.KeyValues = KeyValues;
            this.OriginalValues = OriginalValues;
            this.CurrentValues = CurrentValues;
            this.Immutable = Immutable;
        }

        public ActivityResult Evolve()
        {
            return new ActivityResult()
            {
                Id = this.Id,
                ActorId = this.ActorId,
                ActivityType = this.ActivityType,
                TableName = this.TableName,
                KeyValues = this.KeyValues,
                OriginalValues = this.OriginalValues,
                CurrentValues = this.CurrentValues,
                Immutable = this.Immutable
            };
        }
    }

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

        public AppActivity Devolve()
        {
            var entry = new AppActivity();

            entry.Id = Guid.NewGuid();
            entry.ActorId = ActorId;
            entry.ActivityType = ActivityType;
            entry.TableName = TableName;
            entry.KeyValues = KeyValues.Count == 0 ? null : JsonConvert.SerializeObject(KeyValues);
            entry.OriginalValues = OriginalValues.Count == 0 ? null : JsonConvert.SerializeObject(OriginalValues);
            entry.CurrentValues = CurrentValues.Count == 0 ? null : JsonConvert.SerializeObject(CurrentValues);
            entry.Created = DateTime.Now;
            entry.Immutable = Immutable;

            return entry;
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
