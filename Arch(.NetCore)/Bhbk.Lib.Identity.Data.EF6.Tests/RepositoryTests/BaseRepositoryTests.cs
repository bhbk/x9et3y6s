using AutoMapper;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF6.Infrastructure;
using Bhbk.Lib.Identity.Data.EF6.Tests.Helpers;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Microsoft.Extensions.Configuration;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer(CollectionOrdererHelper.TypeName, CollectionOrdererHelper.AssembyName)]
namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests
{
    [CollectionDefinition("RepositoryTests")]
    public class BaseRepositoryTestsCollection : ICollectionFixture<BaseRepositoryTests> { }

    public class BaseRepositoryTests
    {
        protected IUnitOfWork UoW;
        protected IMapper Mapper;

        public BaseRepositoryTests()
        {
            var file = SearchRoots.ByAssemblyContext("appsettings.json");

            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.IntegrationTest);

            UoW = new UnitOfWork(conf["Databases:IdentityEntities"], instance);
            Mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EF6>()).CreateMapper();
        }
    }
}
