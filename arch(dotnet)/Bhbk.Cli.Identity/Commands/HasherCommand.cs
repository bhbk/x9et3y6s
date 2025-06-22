using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.Identity.Primitives.Enums;
using ManyConsole;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class HasherCommand : ConsoleCommand
    {
        private HashTypes _hashType;
        private string _hashTypeList = string.Join(", ", Enum.GetNames(typeof(HashTypes)));

        public HasherCommand()
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
                if (_hashType == HashTypes.PBKDF2)
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

                if (_hashType == HashTypes.SHA256)
                {
                    Console.Write("Enter plain text value: ");
                    var clearText = StandardInput.GetHiddenInput();
                    var hashText = SHA256.Create(clearText);

                    Console.WriteLine();
                    Console.WriteLine("  Hash value: " + hashText);
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
