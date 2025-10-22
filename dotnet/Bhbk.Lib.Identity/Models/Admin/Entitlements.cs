using System;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class EntitlementTypes
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public int SortOrder { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public bool IsDeletable { get; set; }
    }

    public class EntitlementTypeV1 : EntitlementTypes
    {
        [Required]
        public Guid Id { get; set; }

        public DateTimeOffset Created { get; set; }
    }

    public abstract class EntitlementScopes
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public int SortOrder { get; set; }

        [Required]
        public bool IsEnabled { get; set; }
    }

    public class EntitlementScopeV1 : EntitlementScopes
    {
        [Required]
        public Guid Id { get; set; }

        public DateTimeOffset Created { get; set; }
    }

    public abstract class UserEntitlements
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid EntitlementTypeId { get; set; }

        [Required]
        public Guid EntitlementScopeId { get; set; }

        public Guid? IssuerId { get; set; }

        public Guid? AudienceId { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public bool IsDeletable { get; set; }
    }

    public class UserEntitlementV1 : UserEntitlements
    {
        [Required]
        public Guid Id { get; set; }

        public DateTimeOffset Created { get; set; }

        public string UserName { get; set; }

        public string EntitlementTypeName { get; set; }

        public string EntitlementScopeName { get; set; }

        public string IssuerName { get; set; }

        public string AudienceName { get; set; }
    }

    public abstract class AudienceEntitlements
    {
        [Required]
        public Guid AudienceId { get; set; }

        [Required]
        public Guid EntitlementTypeId { get; set; }

        [Required]
        public Guid EntitlementScopeId { get; set; }

        public Guid? IssuerId { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public bool IsDeletable { get; set; }
    }

    public class AudienceEntitlementV1 : AudienceEntitlements
    {
        [Required]
        public Guid Id { get; set; }

        public DateTimeOffset Created { get; set; }

        public string AudienceName { get; set; }

        public string EntitlementTypeName { get; set; }

        public string EntitlementScopeName { get; set; }

        public string IssuerName { get; set; }
    }
}
