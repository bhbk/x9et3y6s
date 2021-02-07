using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Sts
{
    public class DeviceCodeProvider : BaseProvider
    {
        public DeviceCodeProvider(IConfiguration conf, IContextService env)
            : base(conf, env) { }
    }
}
