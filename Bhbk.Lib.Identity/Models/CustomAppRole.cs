using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identityrole?view=aspnetcore-1.1
    public partial class AppRole : IdentityRole<Guid>
    {

    }

    public abstract class RoleBase
    {
        [Required]
        public Guid AudienceId { get; set; }

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
        public Guid ActorId { get; set; }
    }

    public class RoleResult : RoleBase
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }

        public IList<string> Users { get; set; }
    }

    public class RoleUpdate : RoleBase
    {
        [Required]
        public Guid Id { get; set; }

        public Guid ActorId { get; set; }

        public DateTime Created { get; set; }

        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
