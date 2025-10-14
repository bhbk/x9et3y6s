using ManyConsole;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class EmailDeleteCommand : ConsoleCommand
    {
        public EmailDeleteCommand()
        {
            IsCommand("email-delete", "Delete email");
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
