using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF6.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using System;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests
{
    public class BaseRepositoryTests
    {
        protected IUoWService UoW;
        protected IMapper Mapper;

        public BaseRepositoryTests()
        {
            var instance = new ContextService(InstanceContext.UnitTest);

            UoW = new UoWService(instance);
            Mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>()).CreateMapper();

            /*
             * only test context allowed to run...
             */

            if (instance.InstanceType != InstanceContext.UnitTest)
                throw new NotSupportedException();
        }
    }
}
