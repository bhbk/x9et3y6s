﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models_Tbl
{
    public partial class tbl_AudienceRole
    {
        public Guid AudienceId { get; set; }
        public Guid RoleId { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }

        public virtual tbl_Audience Audience { get; set; }
        public virtual tbl_Role Role { get; set; }
    }
}
