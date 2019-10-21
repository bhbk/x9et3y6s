using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Sts
{
    public class ResourceOwnerProvider : BaseProvider
    {
        public ResourceOwnerProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
