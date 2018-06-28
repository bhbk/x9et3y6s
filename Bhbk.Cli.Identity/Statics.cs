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
        audience,
        client,
        role,
        rolemap,
        user,
        userpass
    }

    internal class Statics
    {
        internal static IdentityContext IoC;
    }
}
