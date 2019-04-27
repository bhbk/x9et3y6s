using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    [Collection("AdminTests")]
    public class RoleServiceTests
    {
        private readonly StartupTests _factory;

        public RoleServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_RoleV1_AddToUser_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var testRole = (await _factory.UoW.RoleRepo.GetAsync(x => x.Name == Strings.ApiUnitTestRole)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Role_AddUserV1(rop.token, testRole.Id, user.Id);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Create_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                /*
                 * check security...
                 */

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Role_CreateV1(rop.token, new RoleCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Role_CreateV1(rop.token, new RoleCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Role_CreateV1(rop.token, new RoleCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Create_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var create = new RoleCreate()
                {
                    ClientId = (await _factory.UoW.ClientRepo.GetAsync()).First().Id,
                    Name = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestRole,
                    Enabled = true,
                    Immutable = false
                };

                var result = await service.HttpClient.Role_CreateV1(rop.token, create);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<RoleModel>().Should().BeAssignableTo<RoleModel>();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Delete_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                /*
                 * check security...
                 */

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Role_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Role_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Role_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                var role1 = (await _factory.UoW.RoleRepo.GetAsync()).First();
                role1.Immutable = true;

                await _factory.UoW.RoleRepo.UpdateAsync(role1);
                await _factory.UoW.CommitAsync();

                result = await service.HttpClient.Role_DeleteV1(rop.token, role1.Id);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Delete_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var role1 = (await _factory.UoW.RoleRepo.GetAsync(x => x.Name == Strings.ApiUnitTestRole)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Role_DeleteV1(rop.token, role1.Id);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = (await _factory.UoW.RoleRepo.GetAsync(x => x.Id == role1.Id)).Any();
                check.Should().BeFalse();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();
                await _factory.TestData.CreateRandomAsync(3);

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var take = 3;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("name", "asc"));

                var result = await service.HttpClient.Role_GetV1(rop.token,
                    new CascadePager()
                    {
                        Filter = string.Empty,
                        Orders = orders,
                        Skip = 1,
                        Take = take,
                    });

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<RoleModel>>();
                var count = (int)ok["count"];

                list.Should().BeAssignableTo<IEnumerable<RoleModel>>();
                list.Count().Should().Be(take);
                count.Should().Be(await _factory.UoW.RoleRepo.CountAsync());

                result = await service.HttpClient.Role_GetV1(rop.token, list.First().Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<RoleModel>().Should().BeAssignableTo<RoleModel>();

                result = await service.HttpClient.Role_GetV1(rop.token, list.First().Name.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<RoleModel>().Should().BeAssignableTo<RoleModel>();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_RemoveFromUser_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();
                var role1 = (await _factory.UoW.RoleRepo.GetAsync(x => x.Name == Strings.ApiUnitTestRole)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Role_RemoveUserV1(rop.token, role1.Id, user1.Id);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Update_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                /*
                 * check security...
                 */

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Role_UpdateV1(rop.token, new RoleModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Role_UpdateV1(rop.token, new RoleModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Role_UpdateV1(rop.token, new RoleModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_RoleV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var role1 = (await _factory.UoW.RoleRepo.GetAsync(x => x.Name == Strings.ApiUnitTestRole)).Single();
                role1.Name += "(Updated)";

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Role_UpdateV1(rop.token, _factory.UoW.Mapper.Map<RoleModel>(role1));

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                var check = ok.ToObject<RoleModel>();

                check.Name.Should().Be(role1.Name);

                await _factory.TestData.DestroyAsync();
            }
        }
    }
}
