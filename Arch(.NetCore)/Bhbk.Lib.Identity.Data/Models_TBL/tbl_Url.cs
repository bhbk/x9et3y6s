using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models_Tbl
{
    public partial class tbl_Url
    {
        public Guid Id { get; set; }
        public Guid AudienceId { get; set; }
        public string UrlHost { get; set; }
        public string UrlPath { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }

        public virtual tbl_Audience Audience { get; set; }
    }
}
