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
using System.Linq.Expressions;

namespace Bhbk.Cli.Identity.Commands
{
    public class UserEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _map;
        private readonly IUnitOfWork _uow;
        private readonly IAdminService _service;
        private E_User _user;
        private bool? _isLockedOut, _isDeletable;

        public UserEditCommand()
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

            IsCommand("user-edit", "Edit user");

            HasRequiredOption("u|user=", "Enter existing user", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<E_User>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<E_User, object>>>()
                        {
                            x => x.UserClaims,
                            x => x.UserLogins,
                            x => x.UserRoles,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** No user '{arg}' ***");
            });

            HasOption("f|first=", "First name", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No first name given ***");

                _user.FirstName = arg;
            });

            HasOption("l|last=", "Last name", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No last name given ***");

                _user.LastName = arg;
            });

            HasOption("E|enabled=", "Is user enabled", arg =>
            {
                _isLockedOut = bool.Parse(arg);
            });

            HasOption("D|deletable=", "Is user deletable", arg =>
            {
                _isDeletable = bool.Parse(arg);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (_isLockedOut.HasValue)
                    _user.IsLockedOut = _isLockedOut.Value;

                if (_isDeletable.HasValue)
                    _user.IsDeletable = _isDeletable.Value;

                var user = _service.User_UpdateV1(_map.Map<UserV1>(_user))
                    .Result;

                FormatOutput.Users(_uow, new List<E_User> { _map.Map<E_User>(user) }, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
