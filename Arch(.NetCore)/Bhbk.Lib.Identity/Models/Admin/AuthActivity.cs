using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class AuthActivity
    {
        public Guid? AudienceId { get; set; }

        public Guid? UserId { get; set; }

        [Required]
        public string LoginType { get; set; }

        [Required]
        public string LoginOutcome { get; set; }

        public string LocalEndpoint { get; set; }

        public string RemoteEndpoint { get; set; }
    }

    public class AuthActivityV1 : AuthActivity
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
