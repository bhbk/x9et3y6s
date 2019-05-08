using Bhbk.Lib.Identity.Data.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Alert
{
    public class EmailProvider : BaseProvider
    {
        public EmailProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }

    }
}
