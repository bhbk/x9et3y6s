using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Models;

namespace Bhbk.Cli.Identity
{
    internal enum ExitCodes : int
    {
        Success = 0,
        Failure = 1,
        Exception = 2
    }

    internal class Statics
    {
        internal static CustomIdentityContext IoC;
    }
}
