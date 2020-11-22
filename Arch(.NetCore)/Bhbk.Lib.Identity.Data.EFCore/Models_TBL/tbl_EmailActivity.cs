using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_TBL
{
    public partial class tbl_EmailActivity
    {
        public Guid Id { get; set; }
        public Guid EmailId { get; set; }
        public string SendgridId { get; set; }
        public string SendgridStatus { get; set; }
        public DateTimeOffset StatusAtUtc { get; set; }

        public virtual tbl_EmailQueue Email { get; set; }
    }
}
