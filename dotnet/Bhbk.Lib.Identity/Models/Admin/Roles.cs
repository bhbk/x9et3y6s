using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models.Admin
{
    public abstract class Roles
    {
        [Required]
        public Guid AudienceId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public bool IsDeletable { get; set; }
    }

    public class RoleV1 : Roles
    {
        [Required]
        public Guid Id { get; set; }

        public DateTimeOffset Created { get; set; }

        public virtual ICollection<UserV1> Users { get; set; }

        public virtual ICollection<AudienceV1> Audiences { get; set; }
    }
}
