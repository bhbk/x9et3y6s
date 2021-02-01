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
    public class AudienceEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _map;
        private readonly IUnitOfWork _uow;
        private readonly IAdminService _service;
        private E_Audience _audience;
        private bool? _isLockedOut, _isDeletable;

        public AudienceEditCommand()
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

            IsCommand("audience-edit", "Edit audience");

            HasRequiredOption("a|audience=", "Enter existing audience", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No audience given ***");

                _audience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<E_Audience>()
                    .Where(x => x.Name == arg).ToLambda(),
                        new List<Expression<Func<E_Audience, object>>>()
                        {
                            x => x.AudienceRoles,
                            x => x.Roles,
                        })
                    .SingleOrDefault();

                if (_audience == null)
                    throw new ConsoleHelpAsException($"  *** No audience '{arg}' ***");
            });

            HasOption("n|name=", "Enter name", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No name given ***");

                _audience.Name = arg;
            });

            HasOption("d|description=", "Enter description", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No description given ***");

                _audience.Description = arg;
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
                    _audience.IsLockedOut = _isLockedOut.Value;

                if (_isDeletable.HasValue)
                    _audience.IsDeletable = _isDeletable.Value;

                var audience = _service.Audience_UpdateV1(_map.Map<AudienceV1>(_audience))
                    .Result;

                StandardOutputFactory.Audiences(_uow, new List<E_Audience> { _map.Map<E_Audience>(audience) }, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
