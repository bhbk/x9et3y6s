using Bhbk.Lib.Core.CommandLine;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class ConfCommands : ConsoleCommand
    {
        private static bool ReadConfig = false;

        public ConfCommands()
        {
            IsCommand("conf", "Do things with conf...");

            HasOption("r|read-config", "Read config data.", arg => { ReadConfig = true; });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var file = SearchRoots.ByAssemblyContext("appsettings.json");

                var conf = (IConfiguration)new ConfigurationBuilder()
                    .SetBasePath(file.DirectoryName)
                    .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                var uow = new UoWService(conf, instance);

                if (ReadConfig)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to read config data...");
                    Console.ReadKey();

                    Console.Write(conf.ToString());

                    Console.WriteLine("\tCompleted read of config data...");
                    Console.WriteLine();
                }

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
