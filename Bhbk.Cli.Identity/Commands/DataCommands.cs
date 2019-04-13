using AutoMapper;
using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Datasets;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Infrastructure;
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
                var lib = SearchRoots.ByAssemblyContext("libsettings.json");

                var conf = new ConfigurationBuilder()
                    .SetBasePath(lib.DirectoryName)
                    .AddJsonFile(lib.Name, optional: false, reloadOnChange: true)
                    .Build();

                var builder = new DbContextOptionsBuilder<IdentityDbContext>()
                    .UseSqlServer(conf["Databases:IdentityEntities"])
                    .EnableSensitiveDataLogging();

                var mapper = new MapperConfiguration(x =>
                {
                    x.AddProfile<IdentityMapper>();
                }).CreateMapper();

                var uow = new IdentityUnitOfWork(builder, ExecutionType.Live, conf, mapper);
                var data = new GenerateDefaultData(uow);

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

                return MessageHelper.FondFarewell();
            }
            catch (Exception ex)
            {
                return MessageHelper.AngryFarewell(ex);
            }
        }
    }
}
