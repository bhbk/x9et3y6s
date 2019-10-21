using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class ClientProvider : BaseProvider
    {
        public ClientProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
