using Bhbk.Lib.Identity.Datasets;
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
        issuer,
        role,
        rolemap,
        user,
        userpass
    }

    internal class Statics
    {
        internal static IdentityContext UoW;
        internal static GenerateDefaultData DefaultData;
    }
}
