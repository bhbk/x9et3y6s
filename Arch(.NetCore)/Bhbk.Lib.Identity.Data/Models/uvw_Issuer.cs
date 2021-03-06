﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class uvw_Issuer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IssuerKey { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
