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
    public class IssuerDeleteCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _map;
        private readonly IUnitOfWork _uow;
        private readonly IAdminService _service;
        private E_Issuer _issuer;

        public IssuerDeleteCommand()
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

            IsCommand("issuer-delete", "Delete issuer");

            HasRequiredOption("i|issuer=", "Enter existing issuer", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No issuer given ***");

                _issuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<E_Issuer>()
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
                FormatOutput.Issuers(_uow, new List<E_Issuer> { _issuer });
                Console.Out.WriteLine();

                Console.Out.Write("  *** Enter 'yes' to delete issuer *** : ");
                var input = StandardInput.GetInput();
                Console.Out.WriteLine();

                if (input.ToLower() == "yes")
                {
                    _ = _service.Issuer_DeleteV1(_issuer.Id)
                        .Result;
                }

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
