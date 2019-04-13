using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.Models
{
    public partial class tbl_Clients
    {
        public tbl_Clients()
        {
            tbl_Activities = new HashSet<tbl_Activities>();
            tbl_Refreshes = new HashSet<tbl_Refreshes>();
            tbl_Roles = new HashSet<tbl_Roles>();
            tbl_States = new HashSet<tbl_States>();
            tbl_Urls = new HashSet<tbl_Urls>();
        }

        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? ActorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClientKey { get; set; }
        public string ClientType { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Issuers Issuer { get; set; }
        public virtual ICollection<tbl_Activities> tbl_Activities { get; set; }
        public virtual ICollection<tbl_Refreshes> tbl_Refreshes { get; set; }
        public virtual ICollection<tbl_Roles> tbl_Roles { get; set; }
        public virtual ICollection<tbl_States> tbl_States { get; set; }
        public virtual ICollection<tbl_Urls> tbl_Urls { get; set; }
    }
}
