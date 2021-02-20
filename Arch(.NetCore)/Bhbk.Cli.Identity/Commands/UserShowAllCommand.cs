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
    public class UserShowAllCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _map;
        private readonly IUnitOfWork _uow;
        private readonly IAdminService _service;
        private IEnumerable<E_User> _users;
        private string _filter;
        private int _count;

        public UserShowAllCommand()
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

            IsCommand("user-show-all", "Show user(s)");

            HasRequiredOption("c|count=", "Enter how many results to display", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _count = int.Parse(arg);
            });

            HasOption("f|filter=", "Enter user (full or partial) name to look for", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _filter = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var expression = QueryExpressionFactory.GetQueryExpression<E_User>();

                if (!string.IsNullOrEmpty(_filter))
                    expression = expression.Where(x => x.UserName.Contains(_filter));

                _users = _uow.Users.Get(expression.ToLambda(),
                    new List<Expression<Func<E_User, object>>>()
                    {
                        x => x.UserClaims,
                        x => x.UserLogins,
                        x => x.UserRoles,
                    })
                    .TakeLast(_count);

                FormatOutput.Users(_uow, _users.OrderBy(x => x.UserName));

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
