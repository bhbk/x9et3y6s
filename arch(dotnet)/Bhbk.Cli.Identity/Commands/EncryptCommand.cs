using Bhbk.Cli.Identity.Constants;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Identity.Domain.Encryption;
using Bhbk.Lib.Identity.Domain.Factories;
using ManyConsole;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class EncryptCommand : ConsoleCommand
    {
        private bool _generateKey;

        public EncryptCommand()
        {
            IsCommand("encrypt", "Encrypt a value using a key");

            HasOption("g|generate-key", "Generate a new encryption key",
                arg => _generateKey = true);
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (_generateKey)
                {
                    var key = AESEncryptionFactory.GenerateKey();

                    Console.WriteLine();
                    Console.WriteLine($"  New encryption key (store in {EncryptionKeyResolver.EnvVarKey}):");
                    Console.WriteLine($"  {key}");
                    Console.WriteLine();

                    return StandardOutput.FondFarewell();
                }

                Console.Write($"Enter encryption key (or press Enter to use {EncryptionKeyResolver.EnvVarKey}): ");
                var inputKey = Console.ReadLine();

                var encryptionKey = string.IsNullOrWhiteSpace(inputKey)
                    ? EncryptionKeyResolver.ResolveKey()
                    : inputKey;

                if (string.IsNullOrEmpty(encryptionKey))
                {
                    Console.WriteLine("Error: No encryption key provided");
                    return 1;
                }

                Console.Write("Enter plain text value to encrypt: ");
                var plaintext = StandardInput.GetHiddenInput();

                Console.WriteLine();

                var encrypted = AESEncryptionFactory.Encrypt(plaintext, encryptionKey);

                Console.WriteLine();
                Console.WriteLine("  Encrypted value (copy to appsettings.json):");
                Console.WriteLine($"  {ConfigConstants.EncryptedValuePrefix}{encrypted}");
                Console.WriteLine();

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
