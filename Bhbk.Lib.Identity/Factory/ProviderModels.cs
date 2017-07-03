using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Factory
{
    public class ProviderCreate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }

    public class ProviderModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
        public IList<UserModel> Users { get; set; }
    }

    public class ProviderUpdate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Immutable { get; set; }
        public bool Enabled { get; set; }
    }
}
