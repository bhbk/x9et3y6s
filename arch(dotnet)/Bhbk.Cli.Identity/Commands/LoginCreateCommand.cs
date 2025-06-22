using AutoMapper;
using Bhbk.Cli.Identity.Factories;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Bhbk.Cli.Identity.Commands
{
    public class LoginCreateCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _map;
        private readonly IUnitOfWork _uow;
        private readonly IAdminService _service;
        private string _loginName;

        public LoginCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            _map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EF>())
                .CreateMapper();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:IdentityEntities_EF"], env);

            _service = new AdminService(_conf)
            {
                Grant = new ResourceOwnerGrantV2(_conf)
            };

            IsCommand("login-create", "Create login");

            HasRequiredOption("l|login=", "Enter new login", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No login given ***");

                _loginName = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var login = _service.Login_CreateV1(
                    new LoginV1()
                    {
                        Name = _loginName,
                        IsEnabled = true,
                        IsDeletable = true,
                    }).Result;

                FormatOutput.Logins(_uow, new List<tbl_Login> { _map.Map<tbl_Login>(login) });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
