using Bhbk.Cli.Identity.Helpers;
using Bhbk.Cli.Identity.Primiitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class UserAddPassCommands : ConsoleCommand
    {
        private static string userName = string.Empty;

        public UserAddPassCommands()
        {
            IsCommand("user-add-pass", "Add password to user");
        }

        public override int Run(string[] remainingArguments)
        {
            UserV1 user = null;

            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService();

                userName = ConsoleHelper.PromptForInput(CommandTypes.user);
                user = admin.User_GetV1(userName).Result;

                if (user != null)
                    Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                        + Environment.NewLine + "\tID is " + user.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find user \"" + userName + "\"");

                var password = ConsoleHelper.PromptForInput(CommandTypes.userpass);

                if (admin.User_SetPasswordV1(user.Id,
                    new PasswordAddV1()
                    {
                        EntityId = user.Id,
                        NewPassword = password,
                        NewPasswordConfirm = password,
                    }).Result)
                    Console.WriteLine(Environment.NewLine + "SUCCESS set password for user \"" + userName + "\"");
                else
                    throw new ConsoleHelpAsException("FAILED set password for user \"" + userName + "\"");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
