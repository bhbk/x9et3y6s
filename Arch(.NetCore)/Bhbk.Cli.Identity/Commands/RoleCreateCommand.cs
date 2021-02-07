using AutoMapper;
using Bhbk.Cli.Identity.Factories;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data_EF6.Infrastructure;
using Bhbk.Lib.Identity.Data_EF6.Models;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Identity.Commands
{
    public class RoleCreateCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _map;
        private readonly IUnitOfWork _uow;
        private readonly IAdminService _service;
        private E_Audience _audience;
        private string _roleName;

        public RoleCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            _map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EF6>())
                .CreateMapper();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:IdentityEntities_EF6"], env);

            _service = new AdminService(_conf)
            {
                Grant = new ResourceOwnerGrantV2(_conf)
            };

            IsCommand("role-create", "Create role");

            HasRequiredOption("a|audience=", "Enter existing audience", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No audience given ***");

                _audience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<E_Audience>()
                    .Where(x => x.Name == arg).ToLambda())
                    .SingleOrDefault();

                if (_audience == null)
                    throw new ConsoleHelpAsException($"  *** No audience '{arg}' ***");
            });

            HasRequiredOption("r|role=", "Enter new role", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No role given ***");

                _roleName = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var role = _service.Role_CreateV1(
                    new RoleV1()
                    {
                        AudienceId = _audience.Id,
                        Name = _roleName,
                        IsEnabled = true,
                        IsDeletable = true,
                    }).Result;

                FormatOutput.Roles(_uow, new List<E_Role> { _map.Map<E_Role>(role) });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
