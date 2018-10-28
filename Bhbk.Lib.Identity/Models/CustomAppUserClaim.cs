using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identityuserclaim?view=aspnetcore-1.1
    public partial class AppUserClaim : IdentityUserClaim<Guid>
    {

    }

    public abstract class UserClaimBase
    {
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
        public Guid ActorId { get; set; }
    }

    public class UserClaimResult : UserClaimBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }

    public class UserClaimUpdate : UserClaimBase
    {
        [Required]
        public Guid Id { get; set; }

        public Guid ActorId { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}