using System;

namespace Bhbk.Cli.Identity.Primitives
{
    internal enum ExitCodes : int
    {
        Success = 0,
        Failure = 1,
        Exception = 2,
    }

    internal enum CommandTypes
    {
        client,
        issuer,
        login,
        role,
        rolemap,
        user,
        userpass
    }
}
