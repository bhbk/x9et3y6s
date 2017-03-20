﻿using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace Bhbk.Lib.Identity.Model
{
    //https://msdn.microsoft.com/en-us/library/microsoft.aspnet.identity.entityframework.identityrole(v=vs.108).aspx
    public partial class AppRole : IdentityRole<Guid, AppUserRole>
    {

    }
}