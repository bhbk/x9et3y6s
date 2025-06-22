using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Domain.Profiles;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class BaseProvider : IDisposable
    {
        protected IUnitOfWork UoW;
        protected IMapper Mapper => new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EF>())
            .CreateMapper();

        protected BaseProvider(IConfiguration conf, IContextService env)
        {
            UoW = new UnitOfWork(conf["Databases:IdentityEntities_EF"], env);
        }

        public void Dispose()
        {
            UoW.Dispose();
        }
    }
}
