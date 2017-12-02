using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Models
{
    public partial class AppUserToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Code { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }

        public AppUser User { get; set; }
    }
}
