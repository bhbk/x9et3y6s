using Bhbk.Lib.Identity.Data.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Me
{
    public class InfoProvider : BaseProvider
    {
        public InfoProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
