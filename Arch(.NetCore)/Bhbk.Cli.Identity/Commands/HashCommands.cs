using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using ManyConsole;
using Microsoft.AspNetCore.Identity;
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
                var file = SearchRoots.ByAssemblyContext("clisettings.json");

                var conf = (IConfiguration)new ConfigurationBuilder()
                    .SetBasePath(file.DirectoryName)
                    .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                var uow = new UoWService(conf, instance);

                if (Generate)
                {
                    Console.WriteLine("Please enter a password...");
                    var cleartext = StandardInput.GetHiddenInput();
                    var hashvalue = uow.Users.passwordHasher.HashPassword(null, cleartext);

                    if (uow.Users.passwordHasher.VerifyHashedPassword(null, hashvalue, cleartext) == PasswordVerificationResult.Failed)
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
