using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.Models
{
    public partial class tbl_Settings
    {
        public Guid Id { get; set; }
        public Guid? IssuerId { get; set; }
        public Guid? ClientId { get; set; }
        public Guid? UserId { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Clients Client { get; set; }
        public virtual tbl_Issuers Issuer { get; set; }
        public virtual tbl_Users User { get; set; }
    }
}
