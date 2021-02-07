using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class ClaimProvider : BaseProvider
    {
        public ClaimProvider(IConfiguration conf, IContextService env)
            : base(conf, env) { }
    }
}
