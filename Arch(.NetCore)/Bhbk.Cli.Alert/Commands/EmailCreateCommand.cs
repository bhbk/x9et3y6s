using ManyConsole;
using System;

namespace Bhbk.Cli.Alert.Commands
{
    public class EmailCreateCommand : ConsoleCommand
    {
        public EmailCreateCommand()
        {
            IsCommand("email-create", "Create email");
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
