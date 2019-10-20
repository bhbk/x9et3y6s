using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
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
    public class LoginServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public LoginServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async ValueTask Admin_LoginV1_Create_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Login_CreateV1(Base64.CreateString(8), new LoginCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                await new TestData(uow, mapper).DestroyAsync();
                await new TestData(uow, mapper).CreateAsync();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Login_CreateV1(rop.RawData, new LoginCreate());
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

                var result = await service.Http.Login_CreateV1(rop.RawData, new LoginCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async ValueTask Admin_LoginV1_Create_Success()
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

                var result = service.Login_CreateV1(
                    new LoginCreate()
                    {
                        Name = Base64.CreateString(4) + "-" + FakeConstants.ApiTestLogin,
                        LoginKey = FakeConstants.ApiTestLoginKey,
                        Enabled = true,
                        Immutable = false
                    });
                result.Should().BeAssignableTo<LoginModel>();

                var check = (await uow.Logins.GetAsync(x => x.Id == result.Id)).Any();
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async ValueTask Admin_LoginV1_Delete_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Login_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                await new TestData(uow, mapper).DestroyAsync();
                await new TestData(uow, mapper).CreateAsync();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Login_DeleteV1(rop.RawData, Guid.NewGuid());
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

                var result = await service.Http.Login_DeleteV1(rop.RawData, Guid.NewGuid());
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

                await new TestData(uow, mapper).DestroyAsync();
                await new TestData(uow, mapper).CreateAsync();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                var testLogin = (await uow.Logins.GetAsync(x => x.Name == FakeConstants.ApiTestLogin)).Single();
                testLogin.Immutable = true;

                await uow.Logins.UpdateAsync(testLogin);
                await uow.CommitAsync();

                var result = await service.Http.Login_DeleteV1(rop.RawData, testLogin.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async ValueTask Admin_LoginV1_Delete_Success()
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

                var testLogin = (await uow.Logins.GetAsync(x => x.Name == FakeConstants.ApiTestLogin)).Single();

                var result = service.Login_DeleteV1(testLogin.Id);
                result.Should().BeTrue();

                var check = (await uow.Logins.GetAsync(x => x.Id == testLogin.Id)).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async ValueTask Admin_LoginV1_Get_Success()
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
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                var testLogin = (await uow.Logins.GetAsync(x => x.Name == FakeConstants.ApiTestLogin)).Single();

                var result = service.Login_GetV1(testLogin.Id.ToString());
                result.Should().BeAssignableTo<LoginModel>();
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
                var user = (await uow.Users.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).Single();

                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                int take = 2;
                var state = new PageState()
                {
                    Sort = new List<PageStateSort>()
                    {
                        new PageStateSort() { Field = "name", Dir = "asc" }
                    },
                    Skip = 0,
                    Take = take
                };

                var result = service.Login_GetV1(state);
                result.Data.ToDynamicList().Count().Should().Be(take);
                result.Total.Should().Be(await uow.Logins.CountAsync());
            }
        }

        [Fact]
        public async ValueTask Admin_LoginV1_Update_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new AdminService(conf, InstanceContext.UnitTest, owin);

                var result = await service.Http.Login_DeleteV1(Base64.CreateString(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                await new TestData(uow, mapper).DestroyAsync();
                await new TestData(uow, mapper).CreateAsync();

                var issuer = (await uow.Issuers.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.Clients.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.Users.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, mapper, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Login_UpdateV1(rop.RawData, new LoginModel());
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

                var result = await service.Http.Login_UpdateV1(rop.RawData, new LoginModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async ValueTask Admin_LoginV1_Update_Success()
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

                var testLogin = (await uow.Logins.GetAsync(x => x.Name == FakeConstants.ApiTestLogin)).Single();
                testLogin.Description += "(Updated)";

                var result = service.Login_UpdateV1(mapper.Map<LoginModel>(testLogin));
                result.Should().BeAssignableTo<LoginModel>();
                result.Description.Should().Be(testLogin.Description);
            }
        }
    }
}
