﻿using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class uvw_Issuers
    {
        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IssuerKey { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }
    }
}