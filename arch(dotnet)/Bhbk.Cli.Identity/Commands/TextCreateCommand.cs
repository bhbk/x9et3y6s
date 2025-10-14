using ManyConsole;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class TextCreateCommand : ConsoleCommand
    {
        public TextCreateCommand()
        {
            IsCommand("text-create", "Create text");
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
