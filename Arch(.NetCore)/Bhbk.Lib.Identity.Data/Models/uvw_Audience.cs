using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class uvw_Audience
    {
        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PasswordHashPBKDF2 { get; set; }
        public string PasswordHashSHA256 { get; set; }
        public string SecurityStamp { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsDeletable { get; set; }
        public int AccessFailedCount { get; set; }
        public int AccessSuccessCount { get; set; }
        public DateTimeOffset? LockoutEndUtc { get; set; }
        public DateTimeOffset? LastLoginSuccessUtc { get; set; }
        public DateTimeOffset? LastLoginFailureUtc { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
