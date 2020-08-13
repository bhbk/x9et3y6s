using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.DataState.Interfaces;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Identity.Commands
{
    public class LoginEditCommands : ConsoleCommand
    {
        public LoginEditCommands()
        {
            IsCommand("login-edit", "Edit login");
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService(conf);
                admin.Grant = new ResourceOwnerGrantV2(conf);

                var logins = admin.Login_GetV1(new DataStateV1()
                {
                    Sort = new List<IDataStateSort>()
                        {
                            new DataStateV1Sort() { Field = "name", Dir = "asc" }
                        },
                    Skip = 0,
                    Take = 100
                }).Result;

                foreach (var loginEntry in logins.Data.OrderBy(x => x.Name))
                    Console.WriteLine($"\t{loginEntry.Name} [{loginEntry.Id}]");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
