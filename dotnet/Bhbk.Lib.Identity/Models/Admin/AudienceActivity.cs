using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public class AudienceActivityV1
    {
        [Required]
        public Guid UserActivityId { get; set; }

        [Required]
        public Guid AudienceId { get; set; }

        public string AudienceName { get; set; }

        public Guid? UserId { get; set; }

        [Required]
        public string LoginType { get; set; }

        [Required]
        public string LoginOutcome { get; set; }

        public string LocalEndpoint { get; set; }

        public string RemoteEndpoint { get; set; }

        [Required]
        public DateTimeOffset Created { get; set; }
    }
}
