using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Url
    {
        public Guid Id { get; set; }
        public Guid AudienceId { get; set; }
        public Guid? ActorId { get; set; }
        public string UrlHost { get; set; }
        public string UrlPath { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual tbl_Audience Audience { get; set; }
    }
}
