﻿using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Interfaces;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Infrastructure_Tbl;
using Bhbk.Lib.Identity.Data.Tests.RepositoryTests_Tbl;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    public class MOTDServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public MOTDServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_MOTDV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateMOTDs();

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == DefaultConstants.UserName_Normal).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testMOTD = uow.MOTDs.Get().First();

                var result = await service.MOTD_GetV1(testMOTD.Id.ToString());
                result.Should().BeAssignableTo<MOTDTssV1>();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(conf, instance.InstanceType, owin)
                {
                    Grant = new ResourceOwnerGrantV2(conf, instance.InstanceType, owin)
                };

                var data = new TestDataFactory(uow);
                data.Destroy();
                data.CreateMOTDs();

                var issuer = uow.Issuers.Get(x => x.Name == DefaultConstants.IssuerName).Single();
                var audience = uow.Audiences.Get(x => x.Name == DefaultConstants.Audience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == DefaultConstants.UserName_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Grant.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                int take = 2;
                var state = new DataStateV1()
                {
                    Sort = new List<IDataStateSort>() {
                        new DataStateV1Sort() { Field = "author", Dir = "asc" }
                    },
                    Skip = 0,
                    Take = take
                };

                var result = await service.MOTD_GetV1(state);
                result.Data.Count().Should().Be(take);
                result.Total.Should().Be(uow.MOTDs.Count());
            }
        }
    }
}
