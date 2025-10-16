using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Test.Identity.Integration.TestingTools;
using Microsoft.Extensions.Configuration;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer(CollectionOrdererHelper.TypeName, CollectionOrdererHelper.AssembyName)]
namespace Bhbk.Test.Identity.Integration.RepositoryTests
{
    [CollectionDefinition("RepositoryTests")]
    public class BaseRepositoryTestsCollection : ICollectionFixture<BaseRepositoryTests> { }

    public class BaseRepositoryTests
    {
        protected IMapper Mapper;
        protected IUnitOfWork UoW;
        protected TestDataSettings TestData;

        public BaseRepositoryTests()
        {
            var conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var testConf = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json", optional: false, reloadOnChange: false)
                .Build();

            TestData = new TestDataSettings();
            testConf.GetSection("TestData").Bind(TestData);

            Mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>()).CreateMapper();

            UoW = new UnitOfWork(conf["Databases:IdentityEntities_EF"],
                new ContextService(InstanceContext.IntegrationTest));
        }
    }
}
