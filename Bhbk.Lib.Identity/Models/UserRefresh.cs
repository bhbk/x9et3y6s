using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
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
        public Guid ActorId { get; set; }

        [Required]
        public string ProtectedTicket { get; set; }
    }

    public class UserRefreshResult : UserRefreshBase
    {
        [Required]
        public Guid Id { get; set; }
    }
}
