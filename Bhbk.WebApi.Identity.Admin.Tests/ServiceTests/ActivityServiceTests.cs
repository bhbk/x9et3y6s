using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    [Collection("AdminTests")]
    public class ActivityServiceTests
    {
        private readonly StartupTests _factory;

        public ActivityServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_ActivityV1_Get_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check model and/or action...
                 */

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("created", "desc"));

                var result = await service.Http.Activity_GetV1(rop.token,
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
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();
                await new TestData(uow).CreateRandomAsync(3);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                service.Jwt = new JwtSecurityToken(rop.token);

                var take = 3;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("created", "asc"));

                for (int i = 0; i <= take; i++)
                    await uow.ActivityRepo.CreateAsync(new ActivityCreate()
                    {
                        UserId = user.Id,
                        ActivityType = "ActivityTest-" + RandomValues.CreateBase64String(32),
                        Immutable = false
                    });

                await uow.CommitAsync();

                var result = service.Activity_GetV1(new CascadePager()
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
