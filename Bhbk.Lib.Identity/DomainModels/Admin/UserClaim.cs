using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
{
    public abstract class UserClaimBase
    {
        public Guid ActorId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public string ClaimIssuer { get; set; }

        public string ClaimOriginalIssuer { get; set; }

        public string ClaimSubject { get; set; }

        [Required]
        public string ClaimType { get; set; }

        [Required]
        public string ClaimValue { get; set; }

        public string ClaimValueType { get; set; }

        public IDictionary<string, string> Properties { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class UserClaimCreate : UserClaimBase
    {

    }

    public class UserClaimModel : UserClaimBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}