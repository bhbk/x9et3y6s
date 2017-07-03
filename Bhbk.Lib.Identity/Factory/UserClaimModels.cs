using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Factory
{
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

    public class UserClaimModel
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
