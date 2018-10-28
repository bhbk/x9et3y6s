using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Identity.Models
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identityuserlogin-1?view=aspnetcore-1.1
    public partial class AppUserLogin : IdentityUserLogin<Guid>
    {

    }

    public abstract class UserLoginBase
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid LoginId { get; set; }

        [Required]
        public string LoginProvider { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ProviderDescription { get; set; }

        public string ProviderKey { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [DefaultValue(false)]
        public bool Immutable { get; set; }
    }

    public class UserLoginCreate : UserLoginBase
    {
        public Guid ActorId { get; set; }
    }

    public class UserLoginResult : UserLoginBase
    {
        [Required]
        public DateTime Created { get; set; }

        [Required]
        public Nullable<DateTime> LastUpdated { get; set; }
    }

    public class UserLoginUpdate : UserLoginBase
    {
        public Guid ActorId { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public Nullable<DateTime> LastUpdated { get; set; }
    }
}
