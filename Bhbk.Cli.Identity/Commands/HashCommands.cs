using Bhbk.Lib.Core.CommandLine;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Infrastructure;
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

                if (Generate)
                {
                    Console.WriteLine("Please enter a password...");
                    var cleartext = StandardInput.GetHiddenInput();
                    var hashvalue = uow.UserRepo.passwordHasher.HashPassword(null, cleartext);

                    if (uow.UserRepo.passwordHasher.VerifyHashedPassword(null, hashvalue, cleartext) == PasswordVerificationResult.Failed)
                        Console.WriteLine("Failed to generate hash. Please try again.");
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Hash Value: " + hashvalue);
                    }
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
