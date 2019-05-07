using Bhbk.Lib.Core.CommandLine;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Helpers;
using ManyConsole;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class DataCommands : ConsoleCommand
    {
        private static bool CreateDefault = false, DestroyDefault = false, DestroyAll = false;

        public DataCommands()
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
                var file = SearchRoots.ByAssemblyContext("appsettings.json");

                var conf = new ConfigurationBuilder()
                    .SetBasePath(file.DirectoryName)
                    .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                var builder = new DbContextOptionsBuilder<_DbContext>()
                    .UseSqlServer(conf["Databases:IdentityEntities"])
                    .EnableSensitiveDataLogging();

                var uow = new UnitOfWork(builder, conf);
                var data = new DefaultData(uow);

                if (CreateDefault)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to create default data...");
                    Console.ReadKey();

                    data.CreateAsync().Wait();

                    Console.WriteLine("\tCompleted create default data...");
                    Console.WriteLine();
                }
                else if (DestroyDefault)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to destroy default data...");
                    Console.ReadKey();

                    data.DestroyAsync().Wait();

                    Console.WriteLine("\tCompleted destroy default data...");
                    Console.WriteLine();
                }
                else if (DestroyAll)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to destroy all data...");
                    Console.ReadKey();

                    data.DestroyAsync().Wait();

                    Console.WriteLine("\tCompleted destroy all data...");
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
