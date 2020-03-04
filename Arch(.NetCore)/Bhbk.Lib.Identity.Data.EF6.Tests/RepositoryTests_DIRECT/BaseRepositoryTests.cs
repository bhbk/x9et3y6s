using AutoMapper;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF6.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests_DIRECT
{
    [CollectionDefinition("RepositoryTests_DIRECT")]
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

            var instance = new ContextService(InstanceContext.UnitTest);

            UoW = new UnitOfWork(conf["Databases:IdentityEntities_DIRECT"], instance);
            Mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EF6_DIRECT>()).CreateMapper();
        }
    }
}
