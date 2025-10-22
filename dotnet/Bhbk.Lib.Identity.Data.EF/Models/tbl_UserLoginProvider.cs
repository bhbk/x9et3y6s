using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_UserLoginProvider
    {
        public Guid UserId { get; set; }
        public Guid LoginProviderId { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset? Created { get; set; }

        public virtual tbl_LoginProvider LoginProvider { get; set; }
        public virtual tbl_User User { get; set; }
    }
}
