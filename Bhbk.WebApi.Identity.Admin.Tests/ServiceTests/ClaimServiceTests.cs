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
    public class ClaimServiceTests
    {
        private readonly StartupTests _factory;

        public ClaimServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_ClaimV1_Create_Fail()
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
                var result = await service.HttpClient.Claim_CreateV1(rop.token, new ClaimCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_CreateV1(rop.token, new ClaimCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_CreateV1(rop.token, new ClaimCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Create_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var create = new ClaimCreate()
                {
                    IssuerId = issuer.Id,
                    Type = Strings.ApiUnitTestClaim,
                    Value = RandomValues.CreateBase64String(8),
                };

                var result = await service.HttpClient.Claim_CreateV1(rop.token, create);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<ClaimModel>().Should().BeAssignableTo<ClaimModel>();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Delete_Fail()
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
                var result = await service.HttpClient.Claim_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                var model = (await _factory.UoW.ClaimRepo.GetAsync()).First();
                model.Immutable = true;

                await _factory.UoW.ClaimRepo.UpdateAsync(model);
                await _factory.UoW.CommitAsync();

                result = await service.HttpClient.Claim_DeleteV1(rop.token, model.Id);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Delete_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var testClaim = (await _factory.UoW.ClaimRepo.GetAsync(x => x.Immutable == false)).First();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Claim_DeleteV1(rop.token, testClaim.Id);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = (await _factory.UoW.ClaimRepo.GetAsync(x => x.Id == testClaim.Id)).Any();
                check.Should().BeFalse();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Get_Success()
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
                orders.Add(new Tuple<string, string>("type", "asc"));

                for (int i = 0; i <= take; i++)
                    await _factory.UoW.ClaimRepo.CreateAsync(new ClaimCreate()
                    {
                        IssuerId = issuer.Id,
                        Type = "ClaimTest-" + RandomValues.CreateBase64String(32),
                        Value = RandomValues.CreateBase64String(8),
                        Immutable = false,
                    });

                await _factory.UoW.CommitAsync();

                var result = await service.HttpClient.Claim_GetV1(rop.token,
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
                var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<ClaimModel>>();
                var count = (int)ok["count"];

                list.Should().BeAssignableTo<IEnumerable<ClaimModel>>();
                list.Count().Should().Be(take);
                count.Should().Be(await _factory.UoW.ClaimRepo.CountAsync());

                result = await service.HttpClient.Claim_GetV1(rop.token, list.First().Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<ClaimModel>().Should().BeAssignableTo<ClaimModel>();

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Update_Fail()
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
                var result = await service.HttpClient.Claim_UpdateV1(rop.token, new ClaimModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_UpdateV1(rop.token, new ClaimModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_UpdateV1(rop.token, new ClaimModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }

        }

        [Fact]
        public async Task Admin_ClaimV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var update = (await _factory.UoW.ClaimRepo.GetAsync(x => x.Immutable == false)).First();
                update.Value += "(Updated)";

                var result = await service.HttpClient.Claim_UpdateV1(rop.token, _factory.UoW.Mapper.Map<ClaimModel>(update));

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                var check = ok.ToObject<ClaimModel>();

                check.Value.Should().Be(update.Value);

                await _factory.TestData.DestroyAsync();
            }
        }
    }
}
