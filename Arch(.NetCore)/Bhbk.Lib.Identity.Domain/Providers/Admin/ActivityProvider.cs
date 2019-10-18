using Bhbk.Lib.Identity.Data.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class ActivityProvider : BaseProvider
    {
        public ActivityProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
