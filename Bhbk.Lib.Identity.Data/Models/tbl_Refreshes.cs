using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class tbl_Refreshes
    {
        public Guid Id { get; set; }
        public Guid IssuerId { get; set; }
        public Guid? ClientId { get; set; }
        public Guid? UserId { get; set; }
        public string RefreshValue { get; set; }
        public string RefreshType { get; set; }
        public DateTime ValidFromUtc { get; set; }
        public DateTime ValidToUtc { get; set; }
        public DateTime Created { get; set; }

        public virtual tbl_Clients Client { get; set; }
        public virtual tbl_Issuers Issuer { get; set; }
        public virtual tbl_Users User { get; set; }
    }
}
