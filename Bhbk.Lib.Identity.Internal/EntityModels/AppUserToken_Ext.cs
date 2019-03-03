using Microsoft.AspNetCore.Identity;
using System;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identityusertoken-1?view=aspnetcore-1.1
    public partial class AppUserToken : IdentityUserToken<Guid>
    {

    }
}
