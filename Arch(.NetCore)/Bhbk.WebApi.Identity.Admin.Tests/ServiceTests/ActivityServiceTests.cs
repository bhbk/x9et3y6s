﻿using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Interfaces;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
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
    public class ActivityServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public ActivityServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_ActivityV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                var testActivity = uow.Activities.Get().First();

                var result = await service.Activity_GetV1(testActivity.Id.ToString());
                result.Should().BeAssignableTo<ActivityV1>();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();

                var service = new AdminService(instance.InstanceType, owin);
                service.Grant = new ResourceOwnerGrantV2(instance.InstanceType, owin);

                new TestDataFactory(uow, mapper).Destroy();
                new TestDataFactory(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.DefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.DefaultAudience_Identity).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.DefaultUser_Admin).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                int take = 2;
                var state = new DataStateV1()
                {
                    Sort = new List<IDataStateSort>() 
                    {
                        new DataStateV1Sort() { Field = "createdUtc", Dir = "asc" }
                    },
                    Skip = 0,
                    Take = take
                };

                var result = await service.Activity_GetV1(state);
                result.Data.Count().Should().Be(take);
                result.Total.Should().Be(uow.Activities.Count());
            }
        }
    }
}
