using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF6.Infrastructure;
using Bhbk.Lib.Identity.Data.EF6.Tests.Helpers;
using Bhbk.Lib.Identity.Domain.Profiles;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer(CollectionOrdererHelper.TypeName, CollectionOrdererHelper.AssembyName)]
namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests
{
    [CollectionDefinition("RepositoryTests")]
    public class BaseRepositoryTestsCollection : ICollectionFixture<BaseRepositoryTests> { }

    public class BaseRepositoryTests : IDisposable
    {
        protected IUnitOfWork UoW;
        protected IMapper Mapper;

        public BaseRepositoryTests()
        {
            var conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.IntegrationTest);

            UoW = new UnitOfWork(conf["Databases:IdentityEntities"], instance);
            Mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EF6>()).CreateMapper();
        }

        public void Dispose()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
        }
    }
}
