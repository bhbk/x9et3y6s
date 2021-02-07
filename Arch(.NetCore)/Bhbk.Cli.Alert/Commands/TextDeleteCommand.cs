using ManyConsole;
using System;

namespace Bhbk.Cli.Alert.Commands
{
    public class TextDeleteCommand : ConsoleCommand
    {
        public TextDeleteCommand()
        {
            IsCommand("text-delete", "Create text");
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
