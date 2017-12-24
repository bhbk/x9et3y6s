using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Identity.Helpers;
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
        private static FileInfo _cf = FileSystemHelper.SearchPaths("appsettings.json");
        private static IConfigurationRoot _cb;
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
                _cb = new ConfigurationBuilder()
                    .SetBasePath(_cf.DirectoryName)
                    .AddJsonFile(_cf.Name)
                    .Build();

                var builder = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(_cb["ConnectionStrings:IdentityEntities"]);

                Statics.Context = new CustomIdentityContext(builder);

                if (Generate)
                {
                    Console.WriteLine("Please enter a password...");
                    var cleartext = PasswordHelper.GetStdin();
                    var hashvalue = Statics.Context.UserMgmt.PasswordHasher.HashPassword(null, cleartext);

                    if (Statics.Context.UserMgmt.PasswordHasher.VerifyHashedPassword(null, hashvalue, cleartext) == PasswordVerificationResult.Failed)
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
