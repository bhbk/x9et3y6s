using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Cryptography.Hashing;
using ManyConsole;
using System;
using Bhbk.Cli.Identity.Primiitives.Enums;

namespace Bhbk.Cli.Identity.Commands
{
    public class HasherCommands : ConsoleCommand
    {
        private static HashTypes _hashType;
        private static string _hashTypeList = string.Join(", ", Enum.GetNames(typeof(HashTypes)));

        public HasherCommands()
        {
            IsCommand("hasher", "Generate hash values");

            HasRequiredOption("t|type=", "Enter hash type", arg =>
            {
                if (!Enum.TryParse(arg, out _hashType))
                    throw new ConsoleHelpAsException($"*** Invalid hash type. Options are '{_hashTypeList}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if(_hashType == HashTypes.PBKDF2)
                {
                    Console.Write("Enter plain text value: ");
                    var clearText = StandardInput.GetHiddenInput();
                    var hashText = PBKDF2.Create(clearText);

                    if (!PBKDF2.Validate(hashText, clearText))
                        Console.WriteLine("Failed to generate hash. Please try again.");
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("  Hash value: " + hashText);
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
