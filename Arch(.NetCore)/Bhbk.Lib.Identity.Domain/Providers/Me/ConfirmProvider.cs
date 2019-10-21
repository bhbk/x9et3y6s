using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Me
{
    public class ConfirmProvider : BaseProvider
    {
        public ConfirmProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
