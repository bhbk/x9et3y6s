using ManyConsole;
using System;

namespace Bhbk.Cli.Alert.Commands
{
    public class AdminCommands : ConsoleCommand
    {
        public AdminCommands()
        {
            IsCommand("admin", "Do things with alert entities...");

        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
