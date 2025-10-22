using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class UserAuthActivity
    {
        public List<Guid> AudienceIds { get; set; } = new List<Guid>();

        public Guid? UserId { get; set; }

        [Required]
        public string LoginType { get; set; }

        [Required]
        public string LoginOutcome { get; set; }

        public string LocalEndpoint { get; set; }

        public string RemoteEndpoint { get; set; }
    }

    public class UserAuthActivityV1 : UserAuthActivity
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset Created { get; set; }
    }
}
