using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    /*
     * moving away from microsoft constructs for identity implementation because of un-needed additional 
     * layers of complexity, and limitations, for the simple operations needing to be performed.
     * 
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identitydbcontext-8?view=aspnetcore-2.0
     */

    public partial class AppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        public AppDbContext(DbContextOptionsBuilder<AppDbContext> optionsBuilder)
            : base(optionsBuilder.Options)
        {

        }
    }
}
