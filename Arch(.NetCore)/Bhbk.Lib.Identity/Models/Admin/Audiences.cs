using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Audiences
    {
        public Guid? ActorId { get; set; }

        [Required]
        public Guid IssuerId { get; set; }

        /*
         * do not allow commas...
         */
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\._\-^%$#!~@?\[\]{}() \\/=+]+$")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool IsLockedOut { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsDeletable { get; set; }

        public string ConcurrencyStamp { get; set; }
        public string SecurityStamp { get; set; }

        public DateTimeOffset CreatedUtc { get; set; }
    }

    public class AudienceV1 : Audiences
    {
        [Required]
        public Guid Id { get; set; }

        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public Nullable<DateTimeOffset> LockoutEndUtc { get; set; }

        public Nullable<DateTimeOffset> LastLoginFailureUtc { get; set; }

        public Nullable<DateTimeOffset> LastLoginSuccessUtc { get; set; }

        public int AccessFailedCount { get; set; }

        public int AccessSuccessCount { get; set; }
    }
}
