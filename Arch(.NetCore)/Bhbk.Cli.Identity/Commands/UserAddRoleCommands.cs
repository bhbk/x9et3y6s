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
    public class UserAddRoleCommands : ConsoleCommand
    {
        private static string userName = string.Empty, roleName = string.Empty;

        public UserAddRoleCommands()
        {
            IsCommand("user-add-role", "Add role to user");
        }

        public override int Run(string[] remainingArguments)
        {
            RoleV1 role = null;
            UserV1 user = null;

            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService(conf)
                {
                    Grant = new ResourceOwnerGrantV2(conf)
                };

                userName = ConsoleHelper.PromptForInput(CommandTypes.user);
                user = admin.User_GetV1(userName).Result;

                if (user != null)
                    Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                        + Environment.NewLine + "\tID is " + user.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find user \"" + userName + "\"");

                roleName = ConsoleHelper.PromptForInput(CommandTypes.role);
                role = admin.Role_GetV1(roleName).Result;

                if (role != null)
                    Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                        + Environment.NewLine + "\tID is " + role.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find role \"" + roleName + "\"");

                if (admin.User_AddToRoleV1(user.Id, role.Id).Result)
                    Console.WriteLine(Environment.NewLine + "SUCCESS add role \"" + roleName + "\" to user \"" + userName + "\"");
                else
                    Console.WriteLine(Environment.NewLine + "FAILED add role \"" + roleName + "\" to user \"" + userName + "\"");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
