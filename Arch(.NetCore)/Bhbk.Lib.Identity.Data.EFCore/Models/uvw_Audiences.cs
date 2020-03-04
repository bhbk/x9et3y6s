using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class uvw_Audiences
    {
        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string AudienceType { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime? LastLoginSuccess { get; set; }
        public DateTime? LastLoginFailure { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }
    }
}
