using Bhbk.Cli.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using ManyConsole;
using System;
using System.Windows.Forms;

namespace Bhbk.Cli.Identity.Cmds
{
    public class HashCmds : ConsoleCommand
    {
        private bool Generate = false;

        public HashCmds()
        {
            IsCommand("hash", "Do things with hashes...");

            HasOption("g|generate", "Generate hash value.", arg => { Generate = true; });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                Statics.context = new CustomIdentityDbContext();
                Statics.uow = new UnitOfWork(Statics.context);

                if (Generate)
                {
                    Console.WriteLine("Please enter a password...");
                    var cleartext = PasswordHelper.GetStdin();
                    var hashvalue = Statics.uow.UserMgmt.PasswordHasher.HashPassword(cleartext);

                    if (!Statics.uow.UserMgmt.PasswordValidator.ValidateAsync(hashvalue).Wait(1000))
                        Console.WriteLine("Failed to generate hash. Please try again.");
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Hash Value: " + hashvalue);

                        Clipboard.SetText(hashvalue);

                        Console.WriteLine();
                        Console.WriteLine("The hash value has been copied to the clipboard.");
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
