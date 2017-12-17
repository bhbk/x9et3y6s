using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Models;
using ManyConsole;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Cmds
{
    public class ConfCmds : ConsoleCommand
    {
        private bool ReadConfig = false;

        public ConfCmds()
        {
            IsCommand("conf", "Do things with conf...");

            HasOption("r|read-config", "Read config data.", arg => { ReadConfig = true; });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(FileSystemHelper.SearchUsualPaths("appsettings.json").DirectoryName)
                    .AddJsonFile("appsettings.json")
                    .Build();

                var builder = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(config["ConnectionStrings:IdentityEntities"]);

                Statics.Context = new CustomIdentityContext(builder);

                if (ReadConfig)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to read config data...");
                    Console.ReadKey();

                    Console.Write(Statics.Context.ConfigMgmt.ToString());

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
