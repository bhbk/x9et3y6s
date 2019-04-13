using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.Models
{
    public partial class tbl_Logins
    {
        public tbl_Logins()
        {
            tbl_UserLogins = new HashSet<tbl_UserLogins>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LoginKey { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Users Actor { get; set; }
        public virtual ICollection<tbl_UserLogins> tbl_UserLogins { get; set; }
    }
}
