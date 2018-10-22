﻿using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Identity.Database;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Models;
using ManyConsole;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Bhbk.Lib.Core.Primitives.Enums;

namespace Bhbk.Cli.Identity.Cmds
{
    public class DataCmds : ConsoleCommand
    {
        private static FileInfo _lib = SearchRoots.ByAssemblyContext("appsettings-lib.json");
        private static IConfigurationRoot _conf;
        private static bool CreateDefault = false, DestroyDefault = false, DestroyAll = false;

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
                _conf = new ConfigurationBuilder()
                    .SetBasePath(_lib.DirectoryName)
                    .AddJsonFile(_lib.Name, optional: false, reloadOnChange: true)
                    .Build();

                var builder = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(_conf["Databases:IdentityEntities"])
                    .EnableSensitiveDataLogging();

                Statics.IoC = new IdentityContext(builder, ContextType.Live);
                Defaults seed = new Defaults(Statics.IoC);

                if (CreateDefault)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to create default data...");
                    Console.ReadKey();

                    seed.Create();

                    Console.WriteLine("\tCompleted create default data...");
                    Console.WriteLine();
                }
                else if (DestroyDefault)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to destroy default data...");
                    Console.ReadKey();

                    seed.Destroy();

                    Console.WriteLine("\tCompleted destroy default data...");
                    Console.WriteLine();
                }
                else if (DestroyAll)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to destroy all data...");
                    Console.ReadKey();

                    seed.Destroy();

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
