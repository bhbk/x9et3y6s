using ManyConsole;
using System;

namespace Bhbk.Cli.Alert.Commands
{
    public class TextCreateCommand : ConsoleCommand
    {
        public TextCreateCommand() 
        {
            IsCommand("text-create", "Text create");
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
