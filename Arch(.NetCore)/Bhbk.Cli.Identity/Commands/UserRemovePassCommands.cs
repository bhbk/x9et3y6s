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
    public class UserRemovePassCommands : ConsoleCommand
    {
        private static string userName = string.Empty;

        public UserRemovePassCommands()
        {
            IsCommand("user-remove-pass", "Remove password from user");
        }

        public override int Run(string[] remainingArguments)
        {
            UserV1 user = null;

            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService(conf);
                admin.Grant = new ResourceOwnerGrantV2(conf);

                userName = ConsoleHelper.PromptForInput(CommandTypes.user);
                user = admin.User_GetV1(userName).Result;

                if (user != null)
                    Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                        + Environment.NewLine + "\tID is " + user.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find user \"" + userName + "\"");

                if (admin.User_RemovePasswordV1(user.Id).Result)
                    Console.WriteLine(Environment.NewLine + "SUCCESS remove password for user \"" + userName + "\"");
                else
                    throw new ConsoleHelpAsException("FAILED remove password for user \"" + userName + "\"");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
