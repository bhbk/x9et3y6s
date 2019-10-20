using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;
using static Bhbk.Lib.DataState.Models.PageState;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    public class ActivityServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public ActivityServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async ValueTask Admin_ActivityV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                await new TestData(uow, mapper).DestroyAsync();
                await new TestData(uow, mapper).CreateAsync();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                var testActivity = (await uow.Activities.GetAsync()).First();

                var result = service.Activity_GetV1(testActivity.Id.ToString());
                result.Should().BeAssignableTo<ActivityModel>();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                await new TestData(uow, mapper).DestroyAsync();
                await new TestData(uow, mapper).CreateAsync();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                int take = 2;
                var state = new PageState()
                {
                    Sort = new List<PageStateSort>() 
                    {
                        new PageStateSort() { Field = "created", Dir = "asc" }
                    },
                    Skip = 0,
                    Take = take
                };

                var result = service.Activity_GetV1(state);
                result.Data.ToDynamicList().Count().Should().Be(take);
                result.Total.Should().Be(await uow.Activities.CountAsync());
            }
        }
    }
}
