using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class LoginProvider : BaseProvider
    {
        public LoginProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
