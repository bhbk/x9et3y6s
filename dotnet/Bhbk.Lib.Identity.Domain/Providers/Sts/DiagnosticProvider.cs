using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Sts
{
    public class DiagnosticProvider : BaseProvider
    {
        public DiagnosticProvider(IConfiguration conf, IContextService env)
            : base(conf, env) { }
    }
}