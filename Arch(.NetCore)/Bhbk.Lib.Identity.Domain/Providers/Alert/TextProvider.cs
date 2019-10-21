using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Alert
{
    public class TextProvider : BaseProvider
    {
        public TextProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }

    }
}
