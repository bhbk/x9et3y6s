using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class uvw_MOTDs
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Quote { get; set; }
        public string Category { get; set; }
        public DateTime? Date { get; set; }
        public string Tags { get; set; }
        public int? Length { get; set; }
        public string Background { get; set; }
    }
}
