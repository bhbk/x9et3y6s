using AutoMapper;
using Bhbk.Cli.Identity.Factories;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
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
    public class IssuerShowCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _map;
        private readonly IUnitOfWork _uow;
        private readonly IAdminService _service;
        private tbl_Issuer _issuer;

        public IssuerShowCommand()
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

            IsCommand("issuer-show", "Show issuer");

            HasRequiredOption("i|issuer=", "Enter existing issuer", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No issuer given ***");

                _issuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                    .Where(x => x.Name == arg).ToLambda())
                    .SingleOrDefault();

                if (_issuer == null)
                    throw new ConsoleHelpAsException($"  *** No issuer '{arg}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                FormatOutput.Issuers(_uow, new List<tbl_Issuer> { _issuer }, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
