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
    public class AudienceCreateCommands : ConsoleCommand
    {
        private static string _issuerName = string.Empty, _audienceName = string.Empty;

        public AudienceCreateCommands()
        {
            IsCommand("audience-create", "Create audience");
        }

        public override int Run(string[] remainingArguments)
        {
            IssuerV1 issuer = null;
            AudienceV1 audience = null;

            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService(conf);
                admin.Grant = new ResourceOwnerGrantV2(conf);

                _issuerName = ConsoleHelper.PromptForInput(CommandTypes.issuer);
                issuer = admin.Issuer_GetV1(_issuerName).Result;

                if (issuer == null)
                    throw new ConsoleHelpAsException("FAILED find issuer \"" + _issuerName + "\"");

                try
                {
                    _audienceName = ConsoleHelper.PromptForInput(CommandTypes.audience);
                    audience = admin.Audience_GetV1(_audienceName).Result;
                }
                catch (Exception) { }

                if (audience != null)
                    Console.WriteLine(Environment.NewLine + "FOUND audience \"" + _audienceName
                        + Environment.NewLine + "\tID is " + audience.Id.ToString());
                else
                {
                    audience = admin.Audience_CreateV1(new AudienceV1()
                    {
                        IssuerId = issuer.Id,
                        Name = _audienceName,
                        IsEnabled = true,
                    }).Result;

                    if (audience != null)
                        Console.WriteLine(Environment.NewLine + "SUCCESS create audience \"" + _audienceName + "\""
                            + Environment.NewLine + "\tID is " + audience.Id.ToString());
                    else
                        throw new ConsoleHelpAsException("FAILED create audience \"" + _audienceName + "\"");
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
