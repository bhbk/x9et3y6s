using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Infrastructure_Tbl;
using Bhbk.Lib.Identity.Domain.Profiles;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Bhbk.Lib.Identity.Data.Tests.RepositoryTests_Tbl
{
    [CollectionDefinition("RepositoryTests_Tbl")]
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

            Mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore_TBL>()).CreateMapper();

            UoW = new UnitOfWork(conf["Databases:IdentityEntities_EFCore_Tbl"],
                new ContextService(InstanceContext.IntegrationTest));
        }
    }
}
