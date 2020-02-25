using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Services;
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
                var file = SearchRoots.ByAssemblyContext("clisettings.json");

                var conf = (IConfiguration)new ConfigurationBuilder()
                    .SetBasePath(file.DirectoryName)
                    .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                var uow = new UoWService(conf["Databases:IdentityEntities"], instance);

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
