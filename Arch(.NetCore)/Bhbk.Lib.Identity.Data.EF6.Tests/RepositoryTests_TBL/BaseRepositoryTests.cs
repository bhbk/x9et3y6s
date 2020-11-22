using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF6.Infrastructure_TBL;
using Bhbk.Lib.Identity.Domain.Profiles;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests_TBL
{
    [CollectionDefinition("RepositoryTests_TBL")]
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

            Mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EF6_TBL>()).CreateMapper();

            UoW = new UnitOfWork(conf["Databases:IdentityEntities_TBL"],
                new ContextService(InstanceContext.IntegrationTest));
        }
    }
}
