using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FakeConstants = Bhbk.Lib.Identity.Internal.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Internal.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    public class ActivityServiceTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _owin;

        public ActivityServiceTests(StartupTests factory)
        {
            _factory = factory;
            _owin = _factory.CreateClient();
        }

        [Fact]
        public async Task Admin_ActivityV1_Get_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                var result = await service.Http.Activity_GetV1(RandomValues.CreateBase64String(8), new CascadePager());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.Activity_GetV1(rop.RawData, new CascadePager());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("created", "desc"));

                var result = await service.Http.Activity_GetV1(rop.RawData,
                    new CascadePager()
                    {
                        Filter = string.Empty,
                        Orders = orders,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_ActivityV1_Get_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                new TestData(uow).CreateRandomAsync(3).Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var take = 2;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("created", "asc"));

                var result = service.Activity_GetV1(
                    new CascadePager()
                        {
                            Filter = string.Empty,
                            Orders = orders,
                            Skip = 1,
                            Take = take,
                        });
                result.Item1.Should().Be(await uow.ActivityRepo.CountAsync());
                result.Item2.Should().BeAssignableTo<IEnumerable<ActivityModel>>();
                result.Item2.Count().Should().Be(take);
            }
        }
    }
}
