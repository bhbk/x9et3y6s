using Bhbk.Lib.Identity.Data.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Sts
{
    public class AuthCodeProvider : BaseProvider
    {
        public AuthCodeProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
