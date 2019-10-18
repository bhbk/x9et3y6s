using Bhbk.Lib.Identity.Data.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class RoleProvider : BaseProvider
    {
        public RoleProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
