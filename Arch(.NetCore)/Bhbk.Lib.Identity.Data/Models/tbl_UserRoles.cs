using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class tbl_UserRoles
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public Guid? ActorId { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Roles Role { get; set; }
        public virtual tbl_Users User { get; set; }
    }
}
