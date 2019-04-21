using AutoMapper;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
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
        protected IUoWService UoW;
        protected IMapper Mapper;
        protected DefaultData DefaultData;
        protected TestData TestData;

        public BaseRepositoryTests()
        {
            var file = SearchRoots.ByAssemblyContext("appsettings.json");

            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.UnitTest);

            UoW = new UoWService(conf, instance);
            Mapper = new MapperConfiguration(x => x.AddProfile<MapperProfile>()).CreateMapper();
            DefaultData = new DefaultData(UoW, Mapper);
            TestData = new TestData(UoW, Mapper);

            /*
             * only test context allowed to run...
             */

            if (instance.InstanceType != InstanceContext.UnitTest)
                throw new NotSupportedException();
        }
    }
}
