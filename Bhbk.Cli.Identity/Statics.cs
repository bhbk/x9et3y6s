using Bhbk.Lib.Identity.Infrastructure;
using System;
using Bhbk.Lib.Identity.Data;

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
        internal static IdentityContext UoW;
        internal static DefaultData DefaultData;
    }
}
