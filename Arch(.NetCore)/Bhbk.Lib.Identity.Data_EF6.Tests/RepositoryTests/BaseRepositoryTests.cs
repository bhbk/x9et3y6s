﻿using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data_EF6.Infrastructure;
using Bhbk.Lib.Identity.Data_EF6.Tests.TestingTools;
using Bhbk.Lib.Identity.Domain.Profiles;
using Microsoft.Extensions.Configuration;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer(CollectionOrdererHelper.TypeName, CollectionOrdererHelper.AssembyName)]
namespace Bhbk.Lib.Identity.Data_EF6.Tests.RepositoryTests
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

            Mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EF6>()).CreateMapper();

            UoW = new UnitOfWork(conf["Databases:IdentityEntities_EF6"],
                new ContextService(InstanceContext.IntegrationTest));
        }
    }
}
