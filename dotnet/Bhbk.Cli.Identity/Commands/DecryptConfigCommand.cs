using Bhbk.Cli.Identity.Constants;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Identity.Domain.Encryption;
using Bhbk.Lib.Identity.Domain.Factories;
using ManyConsole;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Bhbk.Cli.Identity.Commands
{
    public class DecryptConfigCommand : ConsoleCommand
    {
        private string _filePath;
        private string _encryptionKey;
        private bool _dryRun;

        public DecryptConfigCommand()
        {
            IsCommand("decrypt-config", "Decrypt sensitive values in <configuration>.json");

            HasRequiredOption("f|file=", "Path to <configuration>.json file",
                arg => _filePath = arg);

            HasOption("k|key=", $"Encryption key (or uses {EncryptionKeyResolver.EnvVarKey} env var)",
                arg => _encryptionKey = arg);

            HasOption("d|dry-run", "Show decrypted values without modifying file",
                arg => _dryRun = true);
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (string.IsNullOrEmpty(_encryptionKey))
                    _encryptionKey = EncryptionKeyResolver.ResolveKey();

                if (string.IsNullOrEmpty(_encryptionKey))
                {
                    Console.WriteLine($"Error: No encryption key provided. Use --key or set {EncryptionKeyResolver.EnvVarKey}");
                    return 1;
                }

                if (!File.Exists(_filePath))
                {
                    Console.WriteLine($"Error: File not found: {_filePath}");
                    return 1;
                }

                var json = File.ReadAllText(_filePath);
                var root = JObject.Parse(json);

                var decryptedCount = 0;
                var skippedCount = 0;

                Console.WriteLine();
                Console.WriteLine($"Processing: {_filePath}");
                Console.WriteLine();

                foreach (var path in ConfigConstants.SensitivePaths)
                {
                    var token = GetTokenByPath(root, path);

                    if (token == null)
                        continue;

                    if (token is JArray array)
                    {
                        for (int i = 0; i < array.Count; i++)
                        {
                            var value = array[i].Value<string>();

                            if (string.IsNullOrEmpty(value))
                                continue;

                            if (!value.StartsWith(ConfigConstants.EncryptedValuePrefix))
                            {
                                Console.WriteLine($"  [SKIP] {path}[{i}] - not encrypted");
                                skippedCount++;
                                continue;
                            }

                            var encrypted = value.Substring(ConfigConstants.EncryptedValuePrefix.Length);
                            var decrypted = AESEncryptionFactory.Decrypt(encrypted, _encryptionKey);

                            if (_dryRun)
                            {
                                Console.WriteLine($"  [DRY-RUN] {path}[{i}]");
                                Console.WriteLine($"    From: {ConfigConstants.EncryptedValuePrefix}{TruncateValue(encrypted)}");
                                Console.WriteLine($"    To:   {TruncateValue(decrypted)}");
                            }
                            else
                            {
                                array[i] = decrypted;
                                Console.WriteLine($"  [DECRYPTED] {path}[{i}]");
                            }

                            decryptedCount++;
                        }
                    }
                    else if (token is JValue jValue && jValue.Type == JTokenType.String)
                    {
                        var value = jValue.Value<string>();

                        if (string.IsNullOrEmpty(value))
                            continue;

                        if (!value.StartsWith(ConfigConstants.EncryptedValuePrefix))
                        {
                            Console.WriteLine($"  [SKIP] {path} - not encrypted");
                            skippedCount++;
                            continue;
                        }

                        var encrypted = value.Substring(ConfigConstants.EncryptedValuePrefix.Length);
                        var decrypted = AESEncryptionFactory.Decrypt(encrypted, _encryptionKey);

                        if (_dryRun)
                        {
                            Console.WriteLine($"  [DRY-RUN] {path}");
                            Console.WriteLine($"    From: {ConfigConstants.EncryptedValuePrefix}{TruncateValue(encrypted)}");
                            Console.WriteLine($"    To:   {TruncateValue(decrypted)}");
                        }
                        else
                        {
                            jValue.Value = decrypted;
                            Console.WriteLine($"  [DECRYPTED] {path}");
                        }

                        decryptedCount++;
                    }
                }

                foreach (var arrayFieldPath in ConfigConstants.SensitiveArrayFields)
                {
                    var parts = arrayFieldPath.Split(':');
                    if (parts.Length < 2)
                        continue;

                    var fieldName = parts[parts.Length - 1];
                    var arrayPath = string.Join(":", parts, 0, parts.Length - 1);

                    var arrayToken = GetTokenByPath(root, arrayPath);

                    if (arrayToken is not JArray array)
                        continue;

                    for (int i = 0; i < array.Count; i++)
                    {
                        if (array[i] is not JObject obj)
                            continue;

                        var fieldToken = obj[fieldName];

                        if (fieldToken is not JValue jValue || jValue.Type != JTokenType.String)
                            continue;

                        var value = jValue.Value<string>();

                        if (string.IsNullOrEmpty(value))
                            continue;

                        if (!value.StartsWith(ConfigConstants.EncryptedValuePrefix))
                        {
                            Console.WriteLine($"  [SKIP] {arrayPath}[{i}].{fieldName} - not encrypted");
                            skippedCount++;
                            continue;
                        }

                        var encrypted = value.Substring(ConfigConstants.EncryptedValuePrefix.Length);
                        var decrypted = AESEncryptionFactory.Decrypt(encrypted, _encryptionKey);

                        if (_dryRun)
                        {
                            Console.WriteLine($"  [DRY-RUN] {arrayPath}[{i}].{fieldName}");
                            Console.WriteLine($"    From: {ConfigConstants.EncryptedValuePrefix}{TruncateValue(encrypted)}");
                            Console.WriteLine($"    To:   {TruncateValue(decrypted)}");
                        }
                        else
                        {
                            jValue.Value = decrypted;
                            Console.WriteLine($"  [DECRYPTED] {arrayPath}[{i}].{fieldName}");
                        }

                        decryptedCount++;
                    }
                }

                Console.WriteLine();

                if (_dryRun)
                {
                    Console.WriteLine($"Dry run complete. {decryptedCount} value(s) would be decrypted, {skippedCount} skipped.");
                }
                else if (decryptedCount > 0)
                {
                    var output = root.ToString(Formatting.Indented);
                    File.WriteAllText(_filePath, output);
                    Console.WriteLine($"Complete. {decryptedCount} value(s) decrypted, {skippedCount} skipped.");
                    Console.WriteLine($"File updated: {_filePath}");
                }
                else
                {
                    Console.WriteLine($"No values to decrypt. {skippedCount} not encrypted.");
                }

                Console.WriteLine();

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }

        private JToken GetTokenByPath(JObject root, string path)
        {
            var parts = path.Split(':');
            JToken current = root;

            foreach (var part in parts)
            {
                if (current is JObject obj)
                {
                    current = obj[part];

                    if (current == null)
                        return null;
                }
                else
                {
                    return null;
                }
            }

            return current;
        }

        private string TruncateValue(string value, int maxLength = 40)
        {
            if (value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength) + "...";
        }
    }
}
