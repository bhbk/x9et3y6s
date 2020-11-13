using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Login
    {
        public tbl_Login()
        {
            tbl_UserLogins = new HashSet<tbl_UserLogin>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LoginKey { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual tbl_User Actor { get; set; }
        public virtual ICollection<tbl_UserLogin> tbl_UserLogins { get; set; }
    }
}
