using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class AudienceProvider : BaseProvider
    {
        public AudienceProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
