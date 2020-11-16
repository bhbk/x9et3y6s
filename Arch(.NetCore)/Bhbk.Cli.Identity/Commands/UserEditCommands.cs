using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.DataState.Interfaces;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Identity.Commands
{
    public class UserEditCommands : ConsoleCommand
    {
        private static string loginName = string.Empty, userName = string.Empty;

        public UserEditCommands()
        {
            IsCommand("user-edit", "Edit user");
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService();

                var users = admin.User_GetV1(new DataStateV1()
                {
                    Sort = new List<IDataStateSort>()
                        {
                            new DataStateV1Sort() { Field = "userName", Dir = "asc" }
                        },
                    Skip = 0,
                    Take = 100
                }).Result;

                foreach (var userEntry in users.Data.OrderBy(x => x.UserName))
                {
                    Console.WriteLine($"  {userEntry.UserName} [{userEntry.Id}]");

                    var roles = admin.User_GetRolesV1(userEntry.Id.ToString()).Result;

                    foreach (var roleEntry in roles.OrderBy(x => x.Name))
                        Console.WriteLine($"    {roleEntry.Name} [{roleEntry.Id}]");

                    Console.WriteLine();
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
