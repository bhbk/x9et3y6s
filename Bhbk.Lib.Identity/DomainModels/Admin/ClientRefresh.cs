using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
{
    public abstract class ClientRefreshBase
    {
        [Required]
        public Guid IssuerId { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public DateTime IssuedUtc { get; set; }

        [Required]
        public DateTime ExpiresUtc { get; set; }
    }

    public class ClientRefreshCreate : ClientRefreshBase
    {
        [Required]
        public string ProtectedTicket { get; set; }
    }

    public class ClientRefreshModel : ClientRefreshBase
    {
        [Required]
        public Guid Id { get; set; }
    }
}
