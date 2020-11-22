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
    public class RoleCreateCommands : ConsoleCommand
    {
        private static string audienceName = string.Empty, roleName = string.Empty;

        public RoleCreateCommands()
        {
            IsCommand("role-create", "Create role");
        }

        public override int Run(string[] remainingArguments)
        {
            AudienceV1 audience = null;
            RoleV1 role = null;

            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService(conf)
                {
                    Grant = new ResourceOwnerGrantV2(conf)
                };

                audienceName = ConsoleHelper.PromptForInput(CommandTypes.audience);
                audience = admin.Audience_GetV1(audienceName).Result;

                if (audience == null)
                    throw new ConsoleHelpAsException("FAILED find audience \"" + audienceName + "\"");

                try
                {
                    roleName = ConsoleHelper.PromptForInput(CommandTypes.role);
                    role = admin.Role_GetV1(roleName).Result;
                }
                catch (Exception) { }

                if (role != null)
                    Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                        + Environment.NewLine + "\tID is " + role.Id.ToString());
                else
                {
                    role = admin.Role_CreateV1(new RoleV1()
                    {
                        AudienceId = audience.Id,
                        Name = roleName,
                        IsEnabled = true,
                    }).Result;

                    if (role != null)
                        Console.WriteLine(Environment.NewLine + "SUCCESS create role \"" + roleName + "\""
                            + Environment.NewLine + "\tID is " + role.Id.ToString());
                    else
                        throw new ConsoleHelpAsException("FAILED create role \"" + roleName + "\"");
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
