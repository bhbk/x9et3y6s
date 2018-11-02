using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Models;
using ManyConsole;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Bhbk.Cli.Identity.Cmds
{
    public class ConfCmds : ConsoleCommand
    {
        private static IConfigurationRoot _conf;
        private static FileInfo _lib = SearchRoots.ByAssemblyContext("appsettings-lib.json");
        private static bool ReadConfig = false;

        public ConfCmds()
        {
            IsCommand("conf", "Do things with conf...");

            HasOption("r|read-config", "Read config data.", arg => { ReadConfig = true; });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                _conf = new ConfigurationBuilder()
                    .SetBasePath(_lib.DirectoryName)
                    .AddJsonFile(_lib.Name, optional: false, reloadOnChange: true)
                    .Build();

                var builder = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(_conf["Databases:IdentityEntities"])
                    .EnableSensitiveDataLogging();

                Statics.UoW = new IdentityContext(builder, ContextType.Live);

                if (ReadConfig)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to read config data...");
                    Console.ReadKey();

                    Console.Write(Statics.UoW.ConfigRepo.ToString());

                    Console.WriteLine("\tCompleted read of config data...");
                    Console.WriteLine();
                }

                return MessageHelper.FondFarewell();
            }
            catch (Exception ex)
            {
                return MessageHelper.AngryFarewell(ex);
            }
        }
    }
}
