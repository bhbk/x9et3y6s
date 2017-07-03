using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Factory
{
    public class ClientCreate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }

    public class ClientModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
        public IList<AudienceModel> Audiences { get; set; }
    }

    public class ClientUpdate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }
}
