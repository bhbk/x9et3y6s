using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.DomainModels.Admin
{
    public abstract class RoleBase
    {
        public Guid ActorId { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class RoleCreate : RoleBase
    {

    }

    public class RoleModel : RoleBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }

        public ICollection<string> Users { get; set; }
    }
}
