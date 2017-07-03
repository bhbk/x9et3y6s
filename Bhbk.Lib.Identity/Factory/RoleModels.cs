using Bhbk.Lib.Identity.Model;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Factory
{
    public class RoleCreate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
        public Guid AudienceId { get; set; }
    }

    public class RoleModel
    {
        public Guid Id { get; set; }
        public Guid AudienceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }

    public class RoleUpdate
    {
        public Guid AudienceId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }
}
