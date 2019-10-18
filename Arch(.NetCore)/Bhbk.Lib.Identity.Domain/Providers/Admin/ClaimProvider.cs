using Bhbk.Lib.Identity.Data.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class ClaimProvider : BaseProvider
    {
        public ClaimProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
