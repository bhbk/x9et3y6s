using Bhbk.Cli.Identity.Constants;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Identity.Domain.Encryption;
using Bhbk.Lib.Identity.Domain.Factories;
using ManyConsole;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class DecryptCommand : ConsoleCommand
    {
        public DecryptCommand()
        {
            IsCommand("decrypt", "Decrypt a value using a key");
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
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

                Console.Write($"Enter encrypted value (with or without {ConfigConstants.EncryptedValuePrefix} prefix): ");
                var encrypted = Console.ReadLine();

                if (encrypted.StartsWith(ConfigConstants.EncryptedValuePrefix))
                    encrypted = encrypted.Substring(ConfigConstants.EncryptedValuePrefix.Length);

                var plaintext = AESEncryptionFactory.Decrypt(encrypted, encryptionKey);

                Console.WriteLine();
                Console.WriteLine("  Decrypted value:");
                Console.WriteLine($"  {plaintext}");
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
