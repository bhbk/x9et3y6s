using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TUserCodes
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Code { get; set; }
        public string CodeType { get; set; }
        public string IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }

        public virtual TUsers User { get; set; }
    }
}
