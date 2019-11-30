using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class tbl_AudienceRoles
    {
        public Guid AudienceId { get; set; }
        public Guid RoleId { get; set; }
        public Guid? ActorId { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Audiences Audience { get; set; }
        public virtual tbl_Roles Role { get; set; }
    }
}
