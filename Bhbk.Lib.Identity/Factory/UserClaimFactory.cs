using Bhbk.Lib.Identity.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Bhbk.Lib.Identity.Factory
{
    public class UserClaimFactory<T> : AppUserClaim
    {
        public UserClaimFactory(AppUserClaim claim)
        {
            this.UserId = claim.UserId;
            this.ClaimType = claim.ClaimType;
            this.ClaimValue = claim.ClaimValue;
            this.ClaimValueType = claim.ClaimValueType;
        }

        public UserClaimFactory(UserClaimCreate claim)
        {

        }

        public UserClaimFactory(UserClaimUpdate claim)
        {

        }

        public AppUserClaim Devolve()
        {
            return new AppUserClaim
            {
                UserId = this.UserId,
                ClaimType = this.ClaimType,
                ClaimValue = this.ClaimValue,
                ClaimValueType = this.ClaimValueType
            };
        }

        public Claim Evolve()
        {
            return new Claim(this.ClaimType,
                this.ClaimValue,
                this.ClaimValueType);
        }
    }

    public class UserClaimCreate
    {
        public Guid UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public string ClaimValueType { get; set; }
        public string Issuer { get; set; }
        public string OriginalIssuer { get; set; }
        public string Subject { get; set; }
        public IDictionary<string, string> Properties { get; set; }
        public bool Immutable { get; set; }
    }

    public class UserClaimResult
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public string ClaimValueType { get; set; }
        public string Issuer { get; set; }
        public string OriginalIssuer { get; set; }
        public string Subject { get; set; }
        public IDictionary<string, string> Properties { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
    }

    public class UserClaimUpdate
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public string ClaimValueType { get; set; }
        public string Issuer { get; set; }
        public string OriginalIssuer { get; set; }
        public string Subject { get; set; }
        public IDictionary<string, string> Properties { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> LastUpdated { get; set; }
        public bool Immutable { get; set; }
    }
}
