using ManyConsole;
using System;
using System.Collections.Generic;

namespace Bhbk.Cli.Identity
{
    public class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            var commands = GetCommands();
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
        }

        public static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }
    }
}
