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
    public class LoginDeleteCommands : ConsoleCommand
    {
        private static string loginName = string.Empty;

        public LoginDeleteCommands()
        {
            IsCommand("login-delete", "Delete login");
        }

        public override int Run(string[] remainingArguments)
        {
            LoginV1 login = null;

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
                    loginName = InputFactory.PromptForInput(CommandTypes.login);
                    login = admin.Login_GetV1(loginName).Result;
                }
                catch (Exception) { }

                if (login == null)
                    Console.WriteLine(Environment.NewLine + "FAILED find login \"" + loginName + "\"");
                else
                {
                    if (admin.Login_DeleteV1(login.Id).Result)
                        Console.WriteLine(Environment.NewLine + "SUCCESS destroy login \"" + loginName + "\""
                            + Environment.NewLine + "\tID is " + login.Id.ToString());
                    else
                        throw new ConsoleHelpAsException("FAILED destroy login \"" + loginName + "\""
                            + Environment.NewLine + "\tID is " + login.Id.ToString());
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
