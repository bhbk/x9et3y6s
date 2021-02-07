using AutoMapper;
using Bhbk.Cli.Identity.Factories;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data_EF6.Infrastructure;
using Bhbk.Lib.Identity.Data_EF6.Models;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Grants;
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
    public class UserRemoveRoleCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _map;
        private readonly IUnitOfWork _uow;
        private readonly IAdminService _service;
        private E_User _user;
        private E_Role _role;

        public UserRemoveRoleCommand()
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

            IsCommand("user-remove-role", "Remove role from user");

            HasRequiredOption("u|user=", "Enter existing user", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<E_User>()
                    .Where(x => x.UserName == arg).ToLambda())
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** No user '{arg}' ***");
            });

            HasRequiredOption("r|role=", "Enter existing role", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No role given ***");

                _role = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<E_Role>()
                    .Where(x => x.Name == arg).ToLambda())
                    .SingleOrDefault();

                if (_role == null)
                    throw new ConsoleHelpAsException($"  *** No role '{arg}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                _ = _service.User_RemoveFromRoleV1(_user.Id, _role.Id)
                    .Result;

                var user = _service.User_GetV1(_user.Id.ToString())
                    .Result;

                FormatOutput.Users(_uow, new List<E_User> { _map.Map<E_User>(user) });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
