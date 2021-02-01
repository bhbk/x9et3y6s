using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Audiences
    {
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
        public bool IsLockedOut { get; set; }

        [Required]
        public bool IsDeletable { get; set; }

        public string ConcurrencyStamp { get; set; }

        public string SecurityStamp { get; set; }

        public DateTimeOffset CreatedUtc { get; set; }
    }

    public class AudienceV1 : Audiences
    {
        [Required]
        public Guid Id { get; set; }

        public Nullable<DateTimeOffset> LockoutEndUtc { get; set; }

        public virtual ICollection<RoleV1> Roles { get; set; }
    }
}
