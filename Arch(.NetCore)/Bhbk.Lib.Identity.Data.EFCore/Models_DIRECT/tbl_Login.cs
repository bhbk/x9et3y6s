using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Login
    {
        public tbl_Login()
        {
            tbl_UserLogin = new HashSet<tbl_UserLogin>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LoginKey { get; set; }
        public bool Enabled { get; set; }
        public bool Immutable { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_User Actor { get; set; }
        public virtual ICollection<tbl_UserLogin> tbl_UserLogin { get; set; }
    }
}
