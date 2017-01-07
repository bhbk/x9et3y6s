using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace Bhbk.Lib.Identity.Model
{
    //https://msdn.microsoft.com/en-us/library/microsoft.aspnet.identity.entityframework.identityuserclaim(v=vs.108).aspx
    public partial class AppUserClaim : IdentityUserClaim<Guid>
    {

    }
}