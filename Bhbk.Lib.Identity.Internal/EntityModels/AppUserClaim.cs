using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.EntityModels
{
    public partial class AppUserClaim
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ActorId { get; set; }
        public string ClaimIssuer { get; set; }
        public string ClaimOriginalIssuer { get; set; }
        public string ClaimSubject { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public string ClaimValueType { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool Immutable { get; set; }

        public virtual AppUser User { get; set; }
    }
}
