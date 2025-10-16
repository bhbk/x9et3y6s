using ManyConsole;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class TextDeleteCommand : ConsoleCommand
    {
        public TextDeleteCommand()
        {
            IsCommand("text-delete", "Delete text");
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
