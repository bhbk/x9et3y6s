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
using System.Linq.Expressions;

namespace Bhbk.Cli.Identity.Commands
{
    public class UserShowListCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _map;
        private readonly IUnitOfWork _uow;
        private readonly IAdminService _service;
        private IEnumerable<E_User> _users;
        private LambdaExpression _expr;
        private string _search;

        public UserShowListCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            _map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EF6>())
                .CreateMapper();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:IdentityEntities_EF6"], instance);

            _service = new AdminService(_conf)
            {
                Grant = new ResourceOwnerGrantV2(_conf)
            };

            IsCommand("user-show-list", "Show user(s)");

            HasOption("s|search=", "Search existing user(s)", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No search given ***");

                _search = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if(string.IsNullOrEmpty(_search))
                    _expr = QueryExpressionFactory.GetQueryExpression<E_User>().ToLambda();
                else
                    _expr = QueryExpressionFactory.GetQueryExpression<E_User>()
                        .Where(x => x.UserName.Contains(_search)).ToLambda();

                _users = _uow.Users.Get(_expr,
                    new List<Expression<Func<E_User, object>>>()
                    {
                        x => x.UserClaims,
                        x => x.UserLogins,
                        x => x.UserRoles,
                    });

                if (_users == null)
                    throw new ConsoleHelpAsException($"  *** No user contains '{_search}' ***");

                StandardOutputFactory.Users(_uow, _users.OrderBy(x => x.UserName));

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
