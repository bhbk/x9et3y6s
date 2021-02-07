using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Infrastructure_Tbl;
using Bhbk.Lib.Identity.Domain.Profiles;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Domain.Providers.Alert
{
    public class BaseProvider : IDisposable
    {
        protected IUnitOfWork UoW;
        protected IMapper Mapper => new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EFCore_TBL>())
            .CreateMapper();

        protected BaseProvider(IConfiguration conf, IContextService env)
        {
            UoW = new UnitOfWork(conf["Databases:IdentityEntities_EFCore_Tbl"], env);
        }

        public void Dispose()
        {
            UoW.Dispose();
        }
    }
}
