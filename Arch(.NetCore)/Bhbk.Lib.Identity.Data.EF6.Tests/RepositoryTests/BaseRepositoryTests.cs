using AutoMapper;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF6.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests
{
    public class BaseRepositoryTests
    {
        protected IUoWService UoW;
        protected IMapper Mapper;

        public BaseRepositoryTests()
        {
            var file = SearchRoots.ByAssemblyContext("appsettings.json");

            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.UnitTest);

            UoW = new UoWService(conf["Databases:IdentityEntities"], instance);
            Mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore>()).CreateMapper();

            /*
             * only test context allowed to run...
             */

            if (instance.InstanceType != InstanceContext.UnitTest)
                throw new NotSupportedException();
        }
    }
}
