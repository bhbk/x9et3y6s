using AutoMapper;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Domain.Providers.Alert
{
    public class BaseProvider : IAsyncDisposable
    {
        protected IUoWService UoW;
        protected IMapper Mapper => new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>()).CreateMapper();

        protected BaseProvider(IConfiguration conf, IContextService instance)
        {
            UoW = new UoWService(conf, instance);
        }

        public async ValueTask DisposeAsync()
        {
            await UoW.DisposeAsync();
        }
    }
}
