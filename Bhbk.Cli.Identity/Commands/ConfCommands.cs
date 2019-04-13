using AutoMapper;
using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using ManyConsole;
using Microsoft.EntityFrameworkCore;
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

                if (ReadConfig)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to read config data...");
                    Console.ReadKey();

                    Console.Write(uow.ConfigRepo.ToString());

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
