﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class uvw_UserLogin
    {
        public Guid UserId { get; set; }
        public Guid LoginId { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset? CreatedUtc { get; set; }
    }
}
