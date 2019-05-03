using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
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

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    public class RoleServiceTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _owin;

        public RoleServiceTests(StartupTests factory)
        {
            _factory = factory;
            _owin = _factory.CreateClient();
        }

        [Fact]
        public async Task Admin_RoleV1_AddToUser_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testRole = (await uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiDefaultRoleForUser)).Single();
                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = service.Role_AddUserV1(testRole.Id, testUser.Id);
                result.Should().BeTrue();

                var check = await uow.UserRepo.IsInRoleAsync(testUser.Id, testRole.Id);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Create_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var result = await service.Http.Role_CreateV1(RandomValues.CreateBase64String(8), new RoleCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Role_CreateV1(rop.RawData, new RoleCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Role_CreateV1(rop.RawData, new RoleCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Create_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = service.Role_CreateV1(
                    new RoleCreate()
                    {
                        ClientId = client.Id,
                        Name = RandomValues.CreateBase64String(4) + "-" + Constants.ApiUnitTestRole,
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
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var result = await service.Http.Role_DeleteV1(RandomValues.CreateBase64String(8), Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Role_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Role_DeleteV1(rop.RawData, Guid.NewGuid());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                var testRole = (await uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiUnitTestRole)).Single();
                testRole.Immutable = true;

                await uow.RoleRepo.UpdateAsync(testRole);
                await uow.CommitAsync();

                result = await service.Http.Role_DeleteV1(rop.RawData, testRole.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Delete_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testRole = (await uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiUnitTestRole)).Single();

                var result = service.Role_DeleteV1(testRole.Id);
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
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();
                await new TestData(uow).CreateRandomAsync(3);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var take = 3;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("name", "asc"));

                var multiple = service.Role_GetV1(
                    new CascadePager()
                    {
                        Filter = string.Empty,
                        Orders = orders,
                        Skip = 1,
                        Take = take,
                    });
                multiple.Item1.Should().Be(await uow.RoleRepo.CountAsync());
                multiple.Item2.Should().BeAssignableTo<IEnumerable<RoleModel>>();
                multiple.Item2.Count().Should().Be(take);

                var single = service.Role_GetV1(multiple.Item2.First().Id.ToString());
                single.Should().BeAssignableTo<RoleModel>();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_RemoveFromUser_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();
                var testRole = (await uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiUnitTestRole)).Single();

                var result = await service.Http.Role_RemoveUserV1(rop.RawData, testRole.Id, testUser.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Update_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AdminService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var result = await service.Http.Role_UpdateV1(RandomValues.CreateBase64String(8), new RoleModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Role_UpdateV1(rop.RawData, new RoleModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                result = await service.Http.Role_UpdateV1(rop.RawData, new RoleModel());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Update_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var service = new AdminService(uow.InstanceType, _owin);
                service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testRole = (await uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiUnitTestRole)).Single();
                testRole.Description += "(Updated)";

                var result = service.Role_UpdateV1(uow.Mapper.Map<RoleModel>(testRole));
                result.Should().BeAssignableTo<RoleModel>();
                result.Description.Should().Be(testRole.Description);
            }
        }
    }
}
