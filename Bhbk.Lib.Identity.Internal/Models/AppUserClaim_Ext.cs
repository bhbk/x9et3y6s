using Microsoft.AspNetCore.Identity;
using System;

namespace Bhbk.Lib.Identity.Models
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identityuserclaim?view=aspnetcore-1.1
    public partial class AppUserClaim : IdentityUserClaim<Guid>
    {

    }
}
