using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class BaseProvider : IDisposable
    {
        protected IUoWService UoW;
        protected IMapper Mapper => new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore>()).CreateMapper();

        protected BaseProvider(IConfiguration conf, IContextService instance)
        {
            UoW = new UoWService(conf["Databases:IdentityEntities"], instance);
        }

        public void Dispose()
        {
            UoW.Dispose();
        }
    }
}
