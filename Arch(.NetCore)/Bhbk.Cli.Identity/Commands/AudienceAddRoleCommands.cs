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
    public class AudienceAddRoleCommands : ConsoleCommand
    {
        private static string audienceName = string.Empty, roleName = string.Empty;

        public AudienceAddRoleCommands()
        {
            IsCommand("audience-add-role", "Add role to audience");
        }

        public override int Run(string[] remainingArguments)
        {
            RoleV1 role = null;
            AudienceV1 audience = null;

            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService(conf);
                admin.Grant = new ResourceOwnerGrantV2(conf);

                audienceName = ConsoleHelper.PromptForInput(CommandTypes.audience);
                audience = admin.Audience_GetV1(audienceName).Result;

                if (audience != null)
                    Console.WriteLine(Environment.NewLine + "FOUND audience \"" + audienceName + "\""
                        + Environment.NewLine + "\tID is " + audience.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find audience \"" + audienceName + "\"");

                roleName = ConsoleHelper.PromptForInput(CommandTypes.role);
                role = admin.Role_GetV1(roleName).Result;

                if (role != null)
                    Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                        + Environment.NewLine + "\tID is " + role.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find role \"" + roleName + "\"");

                if (admin.Audience_AddToRoleV1(audience.Id, role.Id).Result)
                    Console.WriteLine(Environment.NewLine + "SUCCESS add role \"" + roleName + "\" to audience \"" + audienceName + "\"");
                else
                    Console.WriteLine(Environment.NewLine + "FAILED add role \"" + roleName + "\" to audience \"" + audienceName + "\"");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
