using Bhbk.Lib.Identity.Model;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Factory
{
    public class AudienceCreate
    {
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AudienceType { get; set; }
        public string AudienceKey { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }

    public class AudienceModel
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AudienceType { get; set; }
        public string AudienceKey { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
        public IList<RoleModel> Roles { get; set; }
    }

    public class AudienceUpdate
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AudienceType { get; set; }
        public string AudienceKey { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
    }
}
