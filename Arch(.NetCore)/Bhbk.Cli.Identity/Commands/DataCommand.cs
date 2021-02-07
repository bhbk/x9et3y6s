using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Domain.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class DataCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private bool _create = false, _destroy = false, _destroyAll = false;

        public DataCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            IsCommand("data", "Data seeding");

            HasOption("c|create", "Create default data", arg => 
            { 
                _create = true; 
            });
            HasOption("d|destroy", "Destroy default data", arg => 
            { 
                _destroy = true; 
            });
            HasOption("a|destroy-all", "Destroy all data", arg => 
            { 
                _destroyAll = true; 
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var env = new ContextService(InstanceContext.DeployedOrLocal);
                var uow = new UnitOfWork(_conf["Databases:IdentityEntities_EFCore"], env);
                var data = new DefaultDataFactory(uow);

                if (_create)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to create default data...");
                    Console.ReadKey();

                    data.CreateSettings();
                    data.CreateIssuers();
                    data.CreateAudiences();
                    data.CreateAudienceRoles();
                    data.CreateRoles();
                    data.CreateLogins();
                    data.CreateUsers();
                    data.CreateUserLogins();
                    data.CreateUserRoles();

                    Console.WriteLine("\tCompleted create default data...");
                    Console.WriteLine();
                }
                else if (_destroy)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to destroy default data...");
                    Console.ReadKey();

                    data.Destroy();

                    Console.WriteLine("\tCompleted destroy default data...");
                    Console.WriteLine();
                }
                else if (_destroyAll)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to destroy all data...");
                    Console.ReadKey();

                    data.Destroy();

                    Console.WriteLine("\tCompleted destroy all data...");
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
