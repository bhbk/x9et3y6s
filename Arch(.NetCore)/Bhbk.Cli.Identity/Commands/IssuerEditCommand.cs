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
    public class IssuerEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _map;
        private readonly IUnitOfWork _uow;
        private readonly IAdminService _service;
        private E_Issuer _issuer;
        private bool? _isEnabled, _isDeletable;

        public IssuerEditCommand()
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

            IsCommand("issuer-edit", "Edit issuer");

            HasRequiredOption("i|issuer=", "Enter existing issuer", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No issuer given ***");

                _issuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<E_Issuer>()
                    .Where(x => x.Name == arg).ToLambda(),
                        new List<Expression<Func<E_Issuer, object>>>()
                        {
                            x => x.Audiences,
                            x => x.Claims,
                        })
                    .SingleOrDefault();

                if (_issuer == null)
                    throw new ConsoleHelpAsException($"  *** No issuer '{arg}' ***");
            });

            HasOption("n|name=", "Enter name", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No name given ***");

                _issuer.Name = arg;
            });

            HasOption("d|description=", "Enter description", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No description given ***");

                _issuer.Description = arg;
            });

            HasOption("E|enabled=", "Is user enabled", arg =>
            {
                _isEnabled = bool.Parse(arg);
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
                if (_isEnabled.HasValue)
                    _issuer.IsEnabled = _isEnabled.Value;

                if (_isDeletable.HasValue)
                    _issuer.IsDeletable = _isDeletable.Value;

                var issuer = _service.Issuer_UpdateV1(_map.Map<IssuerV1>(_issuer))
                    .Result;

                StandardOutputFactory.Issuers(_uow, new List<E_Issuer> { _map.Map<E_Issuer>(issuer) }, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
