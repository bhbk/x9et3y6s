using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.EntityModels;
using ManyConsole;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Cmds
{
    public class HashCmds : ConsoleCommand
    {
        private static IConfigurationRoot _conf;
        private static bool Generate = false;

        public HashCmds()
        {
            IsCommand("hash", "Do things with hashes...");

            HasOption("g|generate", "Generate hash value.", arg => { Generate = true; });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var lib = SearchRoots.ByAssemblyContext("libsettings.json");

                _conf = new ConfigurationBuilder()
                    .SetBasePath(lib.DirectoryName)
                    .AddJsonFile(lib.Name, optional: false, reloadOnChange: true)
                    .Build();

                var builder = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(_conf["Databases:IdentityEntities"])
                    .EnableSensitiveDataLogging();

                Statics.UoW = new IdentityContext(builder, ContextType.Live, _conf);

                if (Generate)
                {
                    Console.WriteLine("Please enter a password...");
                    var cleartext = ConsoleHelper.GetHiddenInput();
                    var hashvalue = Statics.UoW.UserRepo.passwordHasher.HashPassword(null, cleartext);

                    if (Statics.UoW.UserRepo.passwordHasher.VerifyHashedPassword(null, hashvalue, cleartext) == PasswordVerificationResult.Failed)
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
