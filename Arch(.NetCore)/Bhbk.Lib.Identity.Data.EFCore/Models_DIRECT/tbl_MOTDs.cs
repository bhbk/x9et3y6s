﻿using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_MOTDs
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