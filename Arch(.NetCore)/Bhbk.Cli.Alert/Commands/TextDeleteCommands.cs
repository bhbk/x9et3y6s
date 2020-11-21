using ManyConsole;
using System;

namespace Bhbk.Cli.Alert.Commands
{
    public class TextDeleteCommands : ConsoleCommand
    {
        public TextDeleteCommands()
        {
            IsCommand("text-delete", "Create text");
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
