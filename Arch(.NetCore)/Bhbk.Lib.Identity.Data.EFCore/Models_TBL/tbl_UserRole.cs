using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_TBL
{
    public partial class tbl_UserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }

        public virtual tbl_Role Role { get; set; }
        public virtual tbl_User User { get; set; }
    }
}
