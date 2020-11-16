using Bhbk.Cli.Identity.Helpers;
using Bhbk.Cli.Identity.Primiitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class LoginCreateCommands : ConsoleCommand
    {
        private static string loginName = string.Empty;

        public LoginCreateCommands()
        {
            IsCommand("login-create", "Create login");
        }

        public override int Run(string[] remainingArguments)
        {
            LoginV1 login = null;

            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService();

                try
                {
                    loginName = ConsoleHelper.PromptForInput(CommandTypes.login);
                    login = admin.Login_GetV1(loginName).Result;
                }
                catch (Exception) { }

                if (login != null)
                    Console.WriteLine(Environment.NewLine + "FOUND login \"" + loginName
                        + Environment.NewLine + "\tID is " + login.Id.ToString());
                else
                {
                    login = admin.Login_CreateV1(new LoginV1()
                    {
                        Name = loginName,
                        IsEnabled = true,
                    }).Result;

                    if (login != null)
                        Console.WriteLine(Environment.NewLine + "SUCCESS create login \"" + loginName + "\""
                            + Environment.NewLine + "\tID is " + login.Id.ToString());
                    else
                        throw new ConsoleHelpAsException("FAILED create login \"" + loginName + "\"");
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
