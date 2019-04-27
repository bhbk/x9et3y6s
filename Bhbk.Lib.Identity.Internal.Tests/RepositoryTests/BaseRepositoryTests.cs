using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Bhbk.Lib.Identity.Internal.Tests.RepositoryTests
{
    [CollectionDefinition("LibraryTests")]
    public class StartupTestsCollection : ICollectionFixture<BaseRepositoryTests> { }

    public class BaseRepositoryTests
    {
        protected IConfigurationRoot Conf;
        protected IIdentityUnitOfWork UoW;
        protected DefaultData DefaultData;
        protected TestData TestData;

        public BaseRepositoryTests()
        {
            var lib = SearchRoots.ByAssemblyContext("config-lib.json");

            var conf = new ConfigurationBuilder()
                .SetBasePath(lib.DirectoryName)
                .AddJsonFile(lib.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .EnableSensitiveDataLogging();

            InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");

            UoW = new IdentityUnitOfWork(options, InstanceContext.UnitTest, conf);
            DefaultData = new DefaultData(UoW);
            TestData = new TestData(UoW);

            /*
             * only test context allowed to run...
             */

            if (UoW.InstanceType != InstanceContext.UnitTest)
                throw new NotSupportedException();
        }
    }
}
