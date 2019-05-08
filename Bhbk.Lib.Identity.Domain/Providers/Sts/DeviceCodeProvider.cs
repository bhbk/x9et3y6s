using Bhbk.Lib.Identity.Data.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Sts
{
    public class DeviceCodeProvider : BaseProvider
    {
        public DeviceCodeProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
