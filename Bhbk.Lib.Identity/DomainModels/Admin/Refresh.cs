using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
{
    public abstract class RefreshBase
    {
        [Required]
        public Guid IssuerId { get; set; }

        public Guid ClientId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        public string RefreshType { get; set; }

        [Required]
        public DateTime IssuedUtc { get; set; }

        [Required]
        public DateTime ExpiresUtc { get; set; }
    }

    public class RefreshCreate : RefreshBase
    {
        [Required]
        public string ProtectedTicket { get; set; }
    }

    public class RefreshModel : RefreshBase
    {
        [Required]
        public Guid Id { get; set; }
    }
}
