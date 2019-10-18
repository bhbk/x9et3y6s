using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static Bhbk.Lib.DataState.Models.PageState;
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    public class ClientServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public ClientServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_ClientV1_Create_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Client_CreateV1(Base64.CreateString(8), new ClientCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Client_CreateV1(rop.RawData, new ClientCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.Client_CreateV1(rop.RawData, new ClientCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_ClientV1_Create_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                var result = service.Client_CreateV1(
                    new ClientCreate()
                    {
                        IssuerId = issuer.Id,
                        Name = Base64.CreateString(4) + "-" + client.Name,
                        ClientKey = AlphaNumeric.CreateString(32),
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                    });
                result.Should().BeAssignableTo<ClientModel>();

                var check = (await uow.Clients.GetAsync(x => x.Id == result.Id)).Any();
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_ClientV1_Delete_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Client_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Client_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.Client_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                var testClient = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
                testClient.Immutable = true;

                uow.Clients.UpdateAsync(testClient).Wait();
                uow.CommitAsync().Wait();

                var result = await service.Http.Client_DeleteV1(rop.RawData, testClient.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_ClientV1_Delete_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                var testClient = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var result = service.Client_DeleteV1(testClient.Id);
                result.Should().BeTrue();

                var check = (await uow.Clients.GetAsync(x => x.Id == testClient.Id)).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_ClientV1_DeleteRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                for (int i = 0; i < 3; i++)
                    await JwtFactory.ClientRefreshV2(uow, mapper, issuer, client);
                uow.CommitAsync().Wait();

                var result = service.Client_DeleteRefreshesV1(client.Id);
                result.Should().BeTrue();

                var check = (await uow.Refreshes.GetAsync(new QueryExpression<tbl_Refreshes>()
                    .Where(x => x.ClientId == client.Id).ToLambda())).Any();
                check.Should().BeFalse();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                await JwtFactory.ClientRefreshV2(uow, mapper, issuer, client);
                uow.CommitAsync().Wait();

                var refresh = (await uow.Refreshes.GetAsync(new QueryExpression<tbl_Refreshes>()
                    .Where(x => x.ClientId == client.Id).ToLambda())).Single();
                var result = service.Client_DeleteRefreshV1(client.Id, refresh.Id);
                result.Should().BeTrue();

                var check = (await uow.Refreshes.GetAsync(new QueryExpression<tbl_Refreshes>()
                    .Where(x => x.Id == refresh.Id).ToLambda())).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_ClientV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                var testClient = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();

                var result = service.Client_GetV1(testClient.Id.ToString());
                result.Should().BeAssignableTo<ClientModel>();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                int take = 2;
                var state = new PageState()
                {
                    Sort = new List<SortModel>()
                    {
                        new SortModel() { Field = "name", Dir = "asc" }
                    },
                    Skip = 0,
                    Take = take
                };

                var result = service.Client_GetV1(state);
                result.Data.ToDynamicList().Count().Should().Be(take);
                result.Total.Should().Be(await uow.Clients.CountAsync());
            }
        }

        [Fact]
        public async Task Admin_ClientV1_GetRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                for (int i = 0; i < 3; i++)
                    await JwtFactory.ClientRefreshV2(uow, mapper, issuer, client);
                uow.CommitAsync().Wait();

                var result = service.Client_GetRefreshesV1(client.Id.ToString());
                result.Should().BeAssignableTo<IEnumerable<RefreshModel>>();
            }
        }

        [Fact]
        public async Task Admin_ClientV1_Update_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Client_UpdateV1(Base64.CreateString(8), new ClientModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Client_UpdateV1(rop.RawData, new ClientModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.Client_UpdateV1(rop.RawData, new ClientModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_ClientV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).DestroyAsync().Wait();
                new TestData(uow, mapper).CreateAsync().Wait();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                var testClient = (await uow.Clients.GetAsync(x => x.Name == FakeConstants.ApiTestClient)).Single();
                testClient.Description += "(Updated)";

                var result = service.Client_UpdateV1(mapper.Map<ClientModel>(testClient));
                result.Should().BeAssignableTo<ClientModel>();
                result.Description.Should().Be(testClient.Description);
            }
        }
    }
}
