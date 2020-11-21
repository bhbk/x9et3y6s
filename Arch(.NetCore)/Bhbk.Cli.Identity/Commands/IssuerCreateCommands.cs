using Bhbk.Cli.Identity.Helpers;
using Bhbk.Cli.Identity.Primiitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class IssuerCreateCommands : ConsoleCommand
    {
        private static string issuerName = string.Empty;

        public IssuerCreateCommands()
        {
            IsCommand("issuer-create", "Create issuer");
        }

        public override int Run(string[] remainingArguments)
        {
            IssuerV1 issuer = null;

            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService(conf);
                admin.Grant = new ResourceOwnerGrantV2(conf);

                issuerName = ConsoleHelper.PromptForInput(CommandTypes.issuer);
                issuer = admin.Issuer_GetV1(issuerName).Result;

                if (issuer != null)
                    Console.WriteLine(Environment.NewLine + "FOUND issuer \"" + issuer.Name + "\""
                        + Environment.NewLine + "\tID is " + issuer.Id.ToString());
                else
                {
                    issuer = admin.Issuer_CreateV1(new IssuerV1()
                    {
                        Name = issuerName,
                        IsEnabled = true,
                    }).Result;

                    if (issuer.Id != Guid.Empty)
                        Console.WriteLine(Environment.NewLine + "SUCCESS create issuer \"" + issuerName + "\""
                            + Environment.NewLine + "\tID is " + issuer.Id.ToString());
                    else
                        throw new ConsoleHelpAsException("FAILED create issuer \"" + issuerName + "\"");
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
