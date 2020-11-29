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
    public class AudienceRemoveRoleCommands : ConsoleCommand
    {
        private static string audienceName = string.Empty, roleName = string.Empty;

        public AudienceRemoveRoleCommands()
        {
            IsCommand("audience-remove-role", "Remove role from audience");
        }

        public override int Run(string[] remainingArguments)
        {
            RoleV1 role = null;
            AudienceV1 user = null;

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
                user = admin.Audience_GetV1(audienceName).Result;

                if (user != null)
                    Console.WriteLine(Environment.NewLine + "FOUND audience \"" + audienceName + "\""
                        + Environment.NewLine + "\tID is " + user.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find audience \"" + audienceName + "\"");

                roleName = InputFactory.PromptForInput(CommandTypes.role);
                role = admin.Role_GetV1(roleName).Result;

                if (role != null)
                    Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                        + Environment.NewLine + "\tID is " + role.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find role \"" + roleName + "\"");

                if (admin.Audience_RemoveFromRoleV1(user.Id, role.Id).Result)
                    Console.WriteLine(Environment.NewLine + "SUCCESS remove role \"" + roleName + "\" from audience \"" + audienceName + "\"");
                else
                    Console.WriteLine(Environment.NewLine + "FAILED remove role \"" + roleName + "\" from audience \"" + audienceName + "\"");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
