using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_TSQL
{
    public partial class uvw_UserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
