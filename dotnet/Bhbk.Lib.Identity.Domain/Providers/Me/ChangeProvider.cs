using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Me
{
    public class ChangeProvider : BaseProvider
    {
        public ChangeProvider(IConfiguration conf, IContextService env)
            : base(conf, env) { }
    }
}
