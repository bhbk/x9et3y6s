using AutoMapper;
using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using ManyConsole;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class HashCommands : ConsoleCommand
    {
        private static bool Generate = false;

        public HashCommands()
        {
            IsCommand("hash", "Do things with hashes...");

            HasOption("g|generate", "Generate hash value.", arg => { Generate = true; });
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

                if (Generate)
                {
                    Console.WriteLine("Please enter a password...");
                    var cleartext = ConsoleHelper.GetHiddenInput();
                    var hashvalue = uow.UserRepo.passwordHasher.HashPassword(null, cleartext);

                    if (uow.UserRepo.passwordHasher.VerifyHashedPassword(null, hashvalue, cleartext) == PasswordVerificationResult.Failed)
                        Console.WriteLine("Failed to generate hash. Please try again.");
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Hash Value: " + hashvalue);
                    }
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
