using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class uvw_EmailActivity
    {
        public Guid Id { get; set; }
        public Guid EmailId { get; set; }
        public string SendgridId { get; set; }
        public string SendgridStatus { get; set; }
        public DateTimeOffset StatusAtUtc { get; set; }
    }
}
