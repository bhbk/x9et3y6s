using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class BaseProvider : IDisposable
    {
        protected IUnitOfWork UoW;
        protected IMapper Mapper => new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore_DIRECT>()).CreateMapper();

        protected BaseProvider(IConfiguration conf, IContextService instance)
        {
            UoW = new UnitOfWork(conf["Databases:IdentityEntities"], instance);
        }

        public void Dispose()
        {
            UoW.Dispose();
        }
    }
}
