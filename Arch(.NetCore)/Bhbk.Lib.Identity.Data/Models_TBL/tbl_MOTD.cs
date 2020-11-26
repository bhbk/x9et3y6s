using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models_TBL
{
    public partial class tbl_MOTD
    {
        public Guid Id { get; set; }
        public string Author { get; set; }
        public string Quote { get; set; }
        public string TssId { get; set; }
        public string TssTitle { get; set; }
        public string TssCategory { get; set; }
        public DateTime? TssDate { get; set; }
        public string TssTags { get; set; }
        public int? TssLength { get; set; }
        public string TssBackground { get; set; }
    }
}
