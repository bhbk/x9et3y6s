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
    public class RoleEditCommands : ConsoleCommand
    {
        private static string audienceName = string.Empty, roleName = string.Empty;

        public RoleEditCommands()
        {
            IsCommand("role-edit", "Edit role");
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var admin = new AdminService();

                var roles = admin.Role_GetV1(new DataStateV1()
                {
                    Sort = new List<IDataStateSort>()
                                {
                                    new DataStateV1Sort() { Field = "name", Dir = "asc" }
                                },
                    Skip = 0,
                    Take = 100
                }).Result;

                foreach (var roleEntry in roles.Data.OrderBy(x => x.Name))
                    Console.WriteLine($"\t{roleEntry.Name} [{roleEntry.Id}]");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
