using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Models;
using ManyConsole;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Bhbk.Cli.Identity.Cmds
{
    public class HashCmds : ConsoleCommand
    {
        private static FileInfo _lib = SearchRoots.ByAssemblyContext("appsettings-lib.json");
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
                _conf = new ConfigurationBuilder()
                    .SetBasePath(_lib.DirectoryName)
                    .AddJsonFile(_lib.Name)
                    .Build();

                var builder = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(_conf["Databases:IdentityEntities"])
                    .EnableSensitiveDataLogging();

                Statics.UoW = new IdentityContext(builder, ContextType.Live);

                if (Generate)
                {
                    Console.WriteLine("Please enter a password...");
                    var cleartext = ConsoleHelper.GetHiddenInput();
                    var hashvalue = Statics.UoW.CustomUserMgr.PasswordHasher.HashPassword(null, cleartext);

                    if (Statics.UoW.CustomUserMgr.PasswordHasher.VerifyHashedPassword(null, hashvalue, cleartext) == PasswordVerificationResult.Failed)
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
