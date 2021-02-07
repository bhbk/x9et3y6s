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
    public class UserCreateCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _map;
        private readonly IUnitOfWork _uow;
        private readonly IAdminService _service;
        private E_Login _login;
        private string _userName, _firstName, _lastName;
        private bool _human;

        public UserCreateCommand()
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

            IsCommand("user-create", "Create user");

            HasRequiredOption("l|login=", "Enter existing login", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No login given ***");

                _login = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                    .Where(x => x.Name == arg).ToLambda())
                    .SingleOrDefault();

                if (_login == null)
                    throw new ConsoleHelpAsException($"  *** No login '{arg}' ***");
            });

            HasRequiredOption("u|user=", "Enter new user", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                _userName = arg;
            });
            
            HasRequiredOption("h|human=", "Is new user human", arg =>
            {
                _human = bool.Parse(arg);
            });

            HasRequiredOption("g|given=", "Given (first) name", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No given (first) name given ***");

                _firstName = arg;
            });

            HasRequiredOption("s|surname=", "Surname (last) name", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No surname (last) name given ***");

                _lastName = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            UserV1 user = null;

            try
            {
                if (_human)
                {
                    user = _service.User_CreateV1(
                        new UserV1()
                        {
                            UserName = _userName,
                            Email = _userName,
                            FirstName = _firstName,
                            LastName = _lastName,
                            IsLockedOut = false,
                            IsHumanBeing = true,
                            IsDeletable = true,
                        }).Result;
                }
                else
                {
                    user = _service.User_CreateV1NoConfirm(
                        new UserV1()
                        {
                            UserName = _userName,
                            Email = _userName,
                            FirstName = _firstName,
                            LastName = _lastName,
                            IsLockedOut = false,
                            IsHumanBeing = false,
                            IsDeletable = true,
                        }).Result;
                }

                _ = _service.User_AddToLoginV1(user.Id, _login.Id)
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
