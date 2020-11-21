using ManyConsole;
using System;

namespace Bhbk.Cli.Alert.Commands
{
    public class EmailCreateCommands : ConsoleCommand
    {
        public EmailCreateCommands()
        {
            IsCommand("email-create", "Create email");
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
