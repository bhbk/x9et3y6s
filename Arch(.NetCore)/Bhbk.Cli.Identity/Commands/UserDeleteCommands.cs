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
    public class UserDeleteCommands : ConsoleCommand
    {
        private static string userName = string.Empty;

        public UserDeleteCommands()
        {
            IsCommand("user-delete", "Delete user");
        }

        public override int Run(string[] remainingArguments)
        {
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

                try
                {
                    userName = InputFactory.PromptForInput(CommandTypes.user);
                    user = admin.User_GetV1(userName).Result;
                }
                catch (Exception) { }

                if (user == null)
                    Console.WriteLine(Environment.NewLine + "FAILED find user \"" + userName + "\"");
                else
                {
                    if (admin.User_DeleteV1(user.Id).Result)
                        Console.WriteLine(Environment.NewLine + "SUCCESS destroy user \"" + userName + "\""
                            + Environment.NewLine + "\tID is " + user.Id.ToString());
                    else
                        throw new ConsoleHelpAsException("FAILED destroy user \"" + userName + "\""
                            + Environment.NewLine + "\tID is \"" + user.Id.ToString() + "\"");
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
