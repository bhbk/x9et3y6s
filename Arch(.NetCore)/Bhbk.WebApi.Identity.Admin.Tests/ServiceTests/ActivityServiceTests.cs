using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;
using static Bhbk.Lib.DataState.Models.PageStateTypeC;

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
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

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
                var service = new AdminService(conf, InstanceContext.End2EndTest, owin);

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiDefaultIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiDefaultAudienceUi).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiDefaultAdminUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.AccessToken = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                int take = 2;
                var state = new PageStateTypeC()
                {
                    Sort = new List<PageStateTypeCSort>() 
                    {
                        new PageStateTypeCSort() { Field = "created", Dir = "asc" }
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
