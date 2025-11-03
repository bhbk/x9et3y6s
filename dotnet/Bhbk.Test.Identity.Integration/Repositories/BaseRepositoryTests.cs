using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Test.Identity.Integration.TestingTools;
using Microsoft.Extensions.Configuration;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer(CollectionOrdererHelper.TypeName, CollectionOrdererHelper.AssembyName)]
namespace Bhbk.Test.Identity.Integration.Repositories
{
    [CollectionDefinition("RepositoryTests")]
    public class BaseRepositoryTestsCollection : ICollectionFixture<BaseRepositoryTests> { }

    public class BaseRepositoryTests
    {
        protected IMapper Mapper;
        protected IUnitOfWork UoW;

        public BaseRepositoryTests()
        {
            var conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>()).CreateMapper();

            UoW = new UnitOfWork(conf["Databases:IdentityEntities_EF"],
                new ContextService(InstanceContext.IntegrationTest));
        }
    }
}
