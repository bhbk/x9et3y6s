using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Factory
{
    public class UserLoginCreate
    {
        public Guid UserId { get; set; }
        public Guid LoginId { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderDescription { get; set; }
        public string ProviderKey { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }

    public class UserLoginModel
    {
        public Guid UserId { get; set; }
        public Guid LoginId { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderDescription { get; set; }
        public string ProviderKey { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
    }

    public class UserLoginUpdate
    {
        public Guid UserId { get; set; }
        public Guid LoginId { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderDescription { get; set; }
        public string ProviderKey { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
    }
}
