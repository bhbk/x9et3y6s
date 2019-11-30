using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Me
{
    public class InitProvider : BaseProvider
    {
        public InitProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
