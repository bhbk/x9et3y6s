using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Domain.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Identity.Commands
{
    public class SeederCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private string _seedSettingsPath;
        private bool _create = false, _destroy = false, _destroyAll = false;

        public SeederCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            IsCommand("seeder", "Data seeding");

            HasOption("s|seed-settings=", "Path to seedsettings.json (default: seedsettings.json)", arg =>
            {
                _seedSettingsPath = arg;
            });
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
                if (string.IsNullOrEmpty(_seedSettingsPath))
                    _seedSettingsPath = "seedsettings.json";

                var seedConf = new ConfigurationBuilder()
                    .AddJsonFile(_seedSettingsPath, optional: false, reloadOnChange: false)
                    .Build();

                var seedData = new SeedDataSettings();
                seedConf.GetSection("SeedData").Bind(seedData);

                var env = new ContextService(InstanceContext.DeployedOrLocal);
                var uow = new UnitOfWork(_conf["Databases:IdentityEntities_EF"], env);
                var data = new DefaultDataFactory(uow, seedData);

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
