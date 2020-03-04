using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Urls
    {
        public Guid Id { get; set; }
        public Guid AudienceId { get; set; }
        public Guid? ActorId { get; set; }
        public string UrlHost { get; set; }
        public string UrlPath { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Audiences Audience { get; set; }
    }
}
