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
    public class DataCmds : ConsoleCommand
    {
        private bool CreateDefault = false, DestroyDefault = false, DestroyAll = false;

        public DataCmds()
        {
            IsCommand("data", "Do things with data...");

            HasOption("c|create-defaults", "Create default data.", arg => { CreateDefault = true; });
            HasOption("d|destroy-defaults", "Destroy default data.", arg => { DestroyDefault = true; });
            HasOption("a|destroy-all", "Destroy all data.", arg => { DestroyAll = true; });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(FileHelper.FindFileInDefaultPaths("appsettings.json").DirectoryName)
                    .AddJsonFile("appsettings.json")
                    .Build();

                var builder = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(config["ConnectionStrings:IdentityEntities"]);

                Statics.Context = new CustomIdentityContext(builder);
                DataHelper seed = new DataHelper(Statics.Context);

                if (CreateDefault)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to create default data...");
                    Console.ReadKey();

                    seed.DefaultDataCreate();

                    Console.WriteLine("\tCompleted create default data...");
                    Console.WriteLine();
                }
                else if (DestroyDefault)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to destroy default data...");
                    Console.ReadKey();

                    seed.DefaultDataDestroy();

                    Console.WriteLine("\tCompleted destroy default data...");
                    Console.WriteLine();
                }
                else if (DestroyAll)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to destroy all data...");
                    Console.ReadKey();

                    seed.CompleteDestroy();

                    Console.WriteLine("\tCompleted destroy all data...");
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
