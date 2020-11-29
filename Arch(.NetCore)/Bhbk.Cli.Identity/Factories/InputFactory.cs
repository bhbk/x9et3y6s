using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using System;

namespace Bhbk.Cli.Identity.Helpers
{
    internal static class InputFactory
    {
        internal static string PromptForInput(CommandTypes cmd)
        {
            switch (cmd)
            {
                case CommandTypes.issuer:
                    Console.Write(Environment.NewLine + "ENTER issuer name : ");
                    return StandardInput.GetInput();

                case CommandTypes.audience:
                    Console.Write(Environment.NewLine + "ENTER audience name : ");
                    return StandardInput.GetInput();

                case CommandTypes.role:
                    Console.Write(Environment.NewLine + "ENTER role name : ");
                    return StandardInput.GetInput();

                case CommandTypes.user:
                    Console.Write(Environment.NewLine + "ENTER user name : ");
                    return StandardInput.GetInput();

                case CommandTypes.userpass:
                    Console.Write(Environment.NewLine + "ENTER user password : ");
                    return StandardInput.GetHiddenInput();
            }

            throw new InvalidOperationException();
        }
    }
}
