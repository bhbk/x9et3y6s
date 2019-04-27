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
using Newtonsoft.Json.Linq;
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
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Claim_CreateV1(rop.token, new ClaimCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_CreateV1(rop.token, new ClaimCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_CreateV1(rop.token, new ClaimCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Create_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var create = new ClaimCreate()
                {
                    IssuerId = issuer.Id,
                    Type = Constants.ApiUnitTestClaim,
                    Value = RandomValues.CreateBase64String(8),
                };

                var result = await service.HttpClient.Claim_CreateV1(rop.token, create);
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<ClaimModel>().Should().BeAssignableTo<ClaimModel>();
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Delete_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Claim_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_DeleteV1(rop.token, Guid.NewGuid());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                var testClaim = (await uow.ClaimRepo.GetAsync()).First();
                testClaim.Immutable = true;

                await uow.ClaimRepo.UpdateAsync(testClaim);
                await uow.CommitAsync();

                result = await service.HttpClient.Claim_DeleteV1(rop.token, testClaim.Id);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Delete_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var testClaim = (await uow.ClaimRepo.GetAsync(x => x.Immutable == false)).First();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Claim_DeleteV1(rop.token, testClaim.Id);

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var check = (await uow.ClaimRepo.GetAsync(x => x.Id == testClaim.Id)).Any();
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Get_Success()
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
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var take = 3;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("type", "asc"));

                for (int i = 0; i <= take; i++)
                    await uow.ClaimRepo.CreateAsync(new ClaimCreate()
                    {
                        IssuerId = issuer.Id,
                        Type = "ClaimTest-" + RandomValues.CreateBase64String(32),
                        Value = RandomValues.CreateBase64String(8),
                        Immutable = false,
                    });

                await uow.CommitAsync();

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
                count.Should().Be(await uow.ClaimRepo.CountAsync());

                result = await service.HttpClient.Claim_GetV1(rop.token, list.First().Id.ToString());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                ok.ToObject<ClaimModel>().Should().BeAssignableTo<ClaimModel>();
            }
        }

        [Fact]
        public async Task Admin_ClaimV1_Update_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                var result = await service.HttpClient.Claim_UpdateV1(rop.token, new ClaimModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_UpdateV1(rop.token, new ClaimModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                /*
                 * check model and/or action...
                 */

                issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);
                result = await service.HttpClient.Claim_UpdateV1(rop.token, new ClaimModel());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

        }

        [Fact]
        public async Task Admin_ClaimV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AdminService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testClaim = (await uow.ClaimRepo.GetAsync(x => x.Immutable == false)).First();
                testClaim.Value += "(Updated)";

                var result = await service.HttpClient.Claim_UpdateV1(rop.token, uow.Mapper.Map<ClaimModel>(testClaim));
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.OK);

                var ok = JObject.Parse(await result.Content.ReadAsStringAsync());
                var check = ok.ToObject<ClaimModel>();
                check.Value.Should().Be(testClaim.Value);
            }
        }
    }
}
