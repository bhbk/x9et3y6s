using Bhbk.Lib.Identity.Infrastructure;
using System;

namespace Bhbk.Cli.Identity
{
    internal enum ExitCodes : int
    {
        Success = 0,
        Failure = 1,
        Exception = 2,
    }

    internal enum CmdType
    {
        client,
        audience,
        role,
        rolemap,
        user,
    }

    internal class Statics
    {
        internal static CustomIdentityContext IoC;
    }
}
