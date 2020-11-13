using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class uvw_MOTD
    {
        public Guid Id { get; set; }
        public string Author { get; set; }
        public string Quote { get; set; }
        public string TssId { get; set; }
        public string TssTitle { get; set; }
        public string TssCategory { get; set; }
        public int? TssLength { get; set; }
        public DateTime? TssDate { get; set; }
        public string TssTags { get; set; }
        public string TssBackground { get; set; }
    }
}
