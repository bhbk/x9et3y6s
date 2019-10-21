using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class UserProvider : BaseProvider
    {
        public UserProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
