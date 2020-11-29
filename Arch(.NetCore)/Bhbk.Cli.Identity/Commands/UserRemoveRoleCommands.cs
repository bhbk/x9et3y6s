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
    public class UserRemoveRoleCommands : ConsoleCommand
    {
        private static string userName = string.Empty, roleName = string.Empty;

        public UserRemoveRoleCommands()
        {
            IsCommand("user-remove-role", "Remove role from user");
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

                userName = InputFactory.PromptForInput(CommandTypes.user);
                user = admin.User_GetV1(userName).Result;

                if (user != null)
                    Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                        + Environment.NewLine + "\tID is " + user.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find user \"" + userName + "\"");

                roleName = InputFactory.PromptForInput(CommandTypes.role);
                role = admin.Role_GetV1(roleName).Result;

                if (role != null)
                    Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                        + Environment.NewLine + "\tID is " + role.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find role \"" + roleName + "\"");

                if (admin.User_RemoveFromRoleV1(user.Id, role.Id).Result)
                    Console.WriteLine(Environment.NewLine + "SUCCESS remove role \"" + roleName + "\" from user \"" + userName + "\"");
                else
                    Console.WriteLine(Environment.NewLine + "FAILED remove role \"" + roleName + "\" from user \"" + userName + "\"");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
