using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class RoleProvider : BaseProvider
    {
        public RoleProvider(IConfiguration conf, IContextService env)
            : base(conf, env) { }
    }
}
