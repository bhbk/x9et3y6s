using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
{
    public abstract class UserRefreshBase
    {
        [Required]
        public Guid IssuerId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime IssuedUtc { get; set; }

        [Required]
        public DateTime ExpiresUtc { get; set; }
    }

    public class UserRefreshCreate : UserRefreshBase
    {
        [Required]
        public string ProtectedTicket { get; set; }
    }

    public class UserRefreshModel : UserRefreshBase
    {
        [Required]
        public Guid Id { get; set; }
    }
}
