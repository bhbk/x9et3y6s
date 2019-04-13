﻿using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.Models
{
    public partial class tbl_Issuers
    {
        public tbl_Issuers()
        {
            tbl_Claims = new HashSet<tbl_Claims>();
            tbl_Clients = new HashSet<tbl_Clients>();
            tbl_Refreshes = new HashSet<tbl_Refreshes>();
            tbl_States = new HashSet<tbl_States>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IssuerKey { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual ICollection<tbl_Claims> tbl_Claims { get; set; }
        public virtual ICollection<tbl_Clients> tbl_Clients { get; set; }
        public virtual ICollection<tbl_Refreshes> tbl_Refreshes { get; set; }
        public virtual ICollection<tbl_States> tbl_States { get; set; }
    }
}
