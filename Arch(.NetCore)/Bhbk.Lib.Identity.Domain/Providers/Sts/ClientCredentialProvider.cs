using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Sts
{
    public class ClientCredentialProvider : BaseProvider
    {
        public ClientCredentialProvider(IConfiguration conf, IContextService env)
            : base(conf, env) { }
    }
}
