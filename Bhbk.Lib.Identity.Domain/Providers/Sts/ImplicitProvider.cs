using Bhbk.Lib.Identity.Data.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Sts
{
    public class ImplicitProvider : BaseProvider
    {
        public ImplicitProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
