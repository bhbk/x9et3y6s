using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Identity.Helpers;
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
        private static FileInfo _cf = FileSystemHelper.SearchPaths("appsettings-lib.json");
        private static IConfigurationRoot _cb;
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
                _cb = new ConfigurationBuilder()
                    .SetBasePath(_cf.DirectoryName)
                    .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                    .Build();

                var builder = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(_cb["Databases:IdentityEntities"])
                    .EnableSensitiveDataLogging();

                Statics.IoC = new IdentityContext(builder);

                if (ReadConfig)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to read config data...");
                    Console.ReadKey();

                    Console.Write(Statics.IoC.ConfigMgmt.ToString());

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
