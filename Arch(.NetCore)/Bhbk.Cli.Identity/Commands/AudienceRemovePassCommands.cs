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
    public class AudienceRemovePassCommands : ConsoleCommand
    {
        private static string audienceName = string.Empty;

        public AudienceRemovePassCommands()
        {
            IsCommand("audience-remove-pass", "Remove password from audience");
        }

        public override int Run(string[] remainingArguments)
        {
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

                audienceName = InputFactory.PromptForInput(CommandTypes.audience);
                audience = admin.Audience_GetV1(audienceName).Result;

                if (audience != null)
                    Console.WriteLine(Environment.NewLine + "FOUND audience \"" + audienceName + "\""
                        + Environment.NewLine + "\tID is " + audience.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find audience \"" + audienceName + "\"");

                if (admin.Audience_RemovePasswordV1(audience.Id).Result)
                    Console.WriteLine(Environment.NewLine + "SUCCESS remove password for audience \"" + audienceName + "\"");
                else
                    throw new ConsoleHelpAsException("FAILED remove password for audience \"" + audienceName + "\"");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
