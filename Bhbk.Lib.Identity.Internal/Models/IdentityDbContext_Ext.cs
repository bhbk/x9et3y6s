using Microsoft.EntityFrameworkCore;
using System;

namespace Bhbk.Lib.Identity.Internal.Models
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identitydbcontext-8?view=aspnetcore-2.0
     */

    public partial class IdentityDbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {

        }

        public IdentityDbContext(DbContextOptionsBuilder<IdentityDbContext> optionsBuilder)
            : base(optionsBuilder.Options)
        {

        }
    }
}
