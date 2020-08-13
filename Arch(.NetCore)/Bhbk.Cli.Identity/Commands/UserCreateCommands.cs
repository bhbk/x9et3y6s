using Bhbk.Cli.Identity.Helpers;
using Bhbk.Cli.Identity.Primiitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class UserCreateCommands : ConsoleCommand
    {
        private static string loginName = string.Empty, userName = string.Empty;

        public UserCreateCommands()
        {
            IsCommand("user-create", "Create user");
        }

        public override int Run(string[] remainingArguments)
        {
            LoginV1 login = null;
            UserV1 user = null;

            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService(conf);
                admin.Grant = new ResourceOwnerGrantV2(conf);

                try
                {
                    userName = ConsoleHelper.PromptForInput(CommandTypes.user);
                    user = admin.User_GetV1(userName).Result;
                }
                catch (Exception) { }

                if (user != null)
                    Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                        + Environment.NewLine + "\tID is " + user.Id.ToString());
                else
                {
                    Console.Write(Environment.NewLine + "ENTER first name : ");
                    var firstName = StandardInput.GetInput();

                    Console.Write(Environment.NewLine + "ENTER last name : ");
                    var lastName = StandardInput.GetInput();

                    user = admin.User_CreateV1NoConfirm(new UserV1()
                    {
                        UserName = userName,
                        Email = userName,
                        FirstName = firstName,
                        LastName = lastName,
                        LockoutEnabled = false,
                        HumanBeing = false,
                        Immutable = false,
                    }).Result;

                    if (user != null)
                        Console.WriteLine(Environment.NewLine + "SUCCESS create user \"" + userName + "\""
                            + Environment.NewLine + "\tID is " + user.Id.ToString());
                    else
                        throw new ConsoleHelpAsException("FAILED create user \"" + userName + "\"");
                }

                loginName = Constants.DefaultLogin;
                login = admin.Login_GetV1(loginName).Result;

                if (login != null)
                    Console.WriteLine(Environment.NewLine + "FOUND login \"" + loginName + "\""
                        + Environment.NewLine + "\tID is " + login.Id.ToString());
                else
                    throw new ConsoleHelpAsException("FAILED find login \"" + loginName + "\"");

                if (admin.User_AddToLoginV1(user.Id, login.Id).Result)
                    Console.WriteLine(Environment.NewLine + "SUCCESS add login \"" + loginName + "\" to user \"" + userName + "\"");
                else
                    throw new ConsoleHelpAsException("FAILED add login \"" + loginName + "\" to user \"" + userName + "\"");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
