using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Infrastructure_Tbl;
using Bhbk.Lib.Identity.Domain.Profiles;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Domain.Providers.Sts
{
    public class BaseProvider : IDisposable
    {
        protected IUnitOfWork UoW;
        protected IMapper Mapper => new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore_TBL>())
            .CreateMapper();

        protected BaseProvider(IConfiguration conf, IContextService instance)
        {
            UoW = new UnitOfWork(conf["Databases:IdentityEntities_EFCore_Tbl"], instance);
        }

        public void Dispose()
        {
            UoW.Dispose();
        }
    }
}
