using System;

namespace Bhbk.Cli.Identity.Primiitives.Enums
{
    internal enum CommandTypes
    {
        audience,
        issuer,
        login,
        role,
        rolemap,
        user,
        userpass
    }

    internal enum HashTypes
    {
        PBKDF2,
    }
}
