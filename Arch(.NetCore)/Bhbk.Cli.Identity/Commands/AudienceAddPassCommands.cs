using Bhbk.Cli.Identity.Helpers;
using Bhbk.Cli.Identity.Primiitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class AudienceAddPassCommands : ConsoleCommand
    {
        private static string audienceName = string.Empty;

        public AudienceAddPassCommands()
        {
            IsCommand("audience-add-pass", "Add password to audience");
        }

        public override int Run(string[] remainingArguments)
        {
            AudienceV1 user = null;

            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService(conf);
                admin.Grant = new ResourceOwnerGrantV2(conf);

                audienceName = ConsoleHelper.PromptForInput(CommandTypes.audience);
                user = admin.Audience_GetV1(audienceName).Result;

                if (user != null)
                    Console.WriteLine(Environment.NewLine + "FOUND audience \"" + audienceName + "\""
                        + Environment.NewLine + "\tID is " + user.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find audience \"" + audienceName + "\"");

                var password = ConsoleHelper.PromptForInput(CommandTypes.userpass);

                if (admin.Audience_SetPasswordV1(user.Id,
                    new PasswordAddV1()
                    {
                        EntityId = user.Id,
                        NewPassword = password,
                        NewPasswordConfirm = password,
                    }).Result)
                    Console.WriteLine(Environment.NewLine + "SUCCESS set password for audience \"" + audienceName + "\"");
                else
                    throw new ConsoleHelpAsException("FAILED set password for audience \"" + audienceName + "\"");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
