using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [CollectionDefinition("LibraryTestsCollection")]
    public class StartupTestsCollection : ICollectionFixture<BaseRepositoryTests> { }

    public class BaseRepositoryTests
    {
        protected IConfiguration Conf;
        protected IUnitOfWork UoW;
        protected DefaultData DefaultData;
        protected TestData TestData;

        public BaseRepositoryTests()
        {
            var file = SearchRoots.ByAssemblyContext("appsettings.json");

            var conf = new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var options = new DbContextOptionsBuilder<_DbContext>()
                .EnableSensitiveDataLogging();

            InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(options, ":InMemory:");

            UoW = new UnitOfWork(options, conf, InstanceContext.UnitTest);
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
