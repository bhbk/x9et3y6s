using AutoMapper;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Core.Primitives.Enums;
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
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    public class RoleServiceTests : IClassFixture<StartupTests>
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _mapper;
        private readonly StartupTests _factory;
        private readonly AdminService _service;

        public RoleServiceTests(StartupTests factory)
        {
            _factory = factory;

            var http = _factory.CreateClient();

            _conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            _mapper = _factory.Server.Host.Services.GetRequiredService<IMapper>();
            _service = new AdminService(_conf, InstanceContext.UnitTest, http);
        }

        [Fact]
        public async Task Admin_RoleV1_Create_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var result = await _service.Http.Role_CreateV1(RandomValues.CreateBase64String(8), new RoleCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var result = await _service.Http.Role_CreateV1(rop.RawData, new RoleCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var result = await _service.Http.Role_CreateV1(rop.RawData, new RoleCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Create_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                _service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var result = _service.Role_CreateV1(
                    new RoleCreate()
                    {
                        ClientId = client.Id,
                        Name = RandomValues.CreateBase64String(4) + "-" + FakeConstants.ApiTestRole,
                        Enabled = true,
                        Immutable = false
                    });
                result.Should().BeAssignableTo<RoleModel>();

                var check = (await uow.RoleRepo.GetAsync(x => x.Id == result.Id)).Any();
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Delete_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var result = await _service.Http.Role_DeleteV1(RandomValues.CreateBase64String(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var result = await _service.Http.Role_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var result = await _service.Http.Role_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var testRole = (await uow.RoleRepo.GetAsync(x => x.Name == FakeConstants.ApiTestRole)).Single();
                testRole.Immutable = true;

                uow.RoleRepo.UpdateAsync(testRole).Wait();
                uow.CommitAsync().Wait();

                var result = await _service.Http.Role_DeleteV1(rop.RawData, testRole.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                new TestData(uow, _mapper).DestroyAsync().Wait();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Delete_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                _service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var testRole = (await uow.RoleRepo.GetAsync(x => x.Name == FakeConstants.ApiTestRole)).Single();

                var result = _service.Role_DeleteV1(testRole.Id);
                result.Should().BeTrue();

                var check = (await uow.RoleRepo.GetAsync(x => x.Id == testRole.Id)).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Get_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateRandomAsync(3).Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).Single();

                _service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var take = 2;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("name", "asc"));

                var result = _service.Role_GetV1(
                    new CascadePager()
                    {
                        Filter = string.Empty,
                        Orders = orders,
                        Skip = 1,
                        Take = take,
                    });
                result.Item1.Should().Be(await uow.RoleRepo.CountAsync());
                result.Item2.Should().BeAssignableTo<IEnumerable<RoleModel>>();
                result.Item2.Count().Should().Be(take);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).Single();

                _service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var testRole = (await uow.RoleRepo.GetAsync(x => x.Name == FakeConstants.ApiTestRole)).Single();

                var result = _service.Role_GetV1(testRole.Id.ToString());
                result.Should().BeAssignableTo<RoleModel>();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Update_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var result = await _service.Http.Role_UpdateV1(RandomValues.CreateBase64String(8), new RoleModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var result = await _service.Http.Role_UpdateV1(rop.RawData, new RoleModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var result = await _service.Http.Role_UpdateV1(rop.RawData, new RoleModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Update_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                _service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var testRole = (await uow.RoleRepo.GetAsync(x => x.Name == FakeConstants.ApiTestRole)).Single();
                testRole.Description += "(Updated)";

                var result = _service.Role_UpdateV1(_mapper.Map<RoleModel>(testRole));
                result.Should().BeAssignableTo<RoleModel>();
                result.Description.Should().Be(testRole.Description);
            }
        }
    }
}
