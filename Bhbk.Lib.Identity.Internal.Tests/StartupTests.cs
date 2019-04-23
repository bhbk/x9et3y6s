using Bhbk.Lib.Identity.Internal.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Bhbk.Lib.Identity.Internal.Tests
{
    [CollectionDefinition("LibraryTests")]
    public class StartupTestsCollection : ICollectionFixture<StartupTests> { }

    public class StartupTests
    {
        public StartupTests()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .EnableSensitiveDataLogging();

            InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");
        }
    }
}
