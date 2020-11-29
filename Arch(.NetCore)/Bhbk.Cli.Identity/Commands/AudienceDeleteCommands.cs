using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class AudienceDeleteCommands : ConsoleCommand
    {
        private static string _issuerName = string.Empty, _audienceName = string.Empty;

        public AudienceDeleteCommands()
        {
            IsCommand("audience-delete", "Delete audience");
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

                var admin = new AdminService(conf)
                {
                    Grant = new ResourceOwnerGrantV2(conf)
                };

                _issuerName = InputFactory.PromptForInput(CommandTypes.issuer);
                issuer = admin.Issuer_GetV1(_issuerName).Result;

                if (issuer == null)
                    throw new ConsoleHelpAsException("FAILED find issuer \"" + _issuerName + "\"");

                try
                {
                    _audienceName = InputFactory.PromptForInput(CommandTypes.audience);
                    audience = admin.Audience_GetV1(_audienceName).Result;
                }
                catch (Exception) { }

                if (audience == null)
                    Console.WriteLine(Environment.NewLine + "FAILED find audience \"" + _audienceName + "\"");
                else
                {
                    if (admin.Audience_DeleteV1(audience.Id).Result)
                        Console.WriteLine(Environment.NewLine + "SUCCESS destroy audience \"" + _audienceName + "\""
                            + Environment.NewLine + "\tID is " + audience.Id.ToString());
                    else
                        throw new ConsoleHelpAsException("FAILED destroy audience \"" + _audienceName + "\""
                            + Environment.NewLine + "\tID is " + audience.Id.ToString());
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
