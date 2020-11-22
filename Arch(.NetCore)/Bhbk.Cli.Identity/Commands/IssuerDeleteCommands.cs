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
    public class IssuerDeleteCommands : ConsoleCommand
    {
        private static string issuerName = string.Empty;

        public IssuerDeleteCommands()
        {
            IsCommand("issuer-delete", "Delete issuer");
        }

        public override int Run(string[] remainingArguments)
        {
            IssuerV1 issuer = null;

            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService(conf)
                {
                    Grant = new ResourceOwnerGrantV2(conf)
                };

                issuerName = ConsoleHelper.PromptForInput(CommandTypes.issuer);
                issuer = admin.Issuer_GetV1(issuerName).Result;

                if (issuer == null)
                    throw new ConsoleHelpAsException("FAILED find issuer \"" + issuerName + "\"");
                else
                {
                    if (admin.Issuer_DeleteV1(issuer.Id).Result)
                        Console.WriteLine(Environment.NewLine + "SUCCESS destroy issuer \"" + issuerName + "\""
                            + Environment.NewLine + "\tID is " + issuer.Id.ToString());
                    else
                        throw new ConsoleHelpAsException("FAILED destroy issuer \"" + issuerName + "\""
                            + Environment.NewLine + "\tID is " + issuer.Id.ToString());
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
