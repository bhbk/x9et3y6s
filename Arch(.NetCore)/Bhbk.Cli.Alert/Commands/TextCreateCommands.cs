using ManyConsole;
using System;

namespace Bhbk.Cli.Alert.Commands
{
    public class TextCreateCommands : ConsoleCommand
    {
        public TextCreateCommands() 
        {
            IsCommand("text-create", "Text create");
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
