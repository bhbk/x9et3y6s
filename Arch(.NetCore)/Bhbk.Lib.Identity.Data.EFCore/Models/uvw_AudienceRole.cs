using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class uvw_AudienceRole
    {
        public Guid AudienceId { get; set; }
        public Guid RoleId { get; set; }
        public Guid? ActorId { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
