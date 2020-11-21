using ManyConsole;
using System;

namespace Bhbk.Cli.Alert.Commands
{
    public class EmailDeleteCommands : ConsoleCommand
    {
        public EmailDeleteCommands()
        {
            IsCommand("email-delete", "Delete email");
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
