using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Sts;
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

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    [Collection("StsTests")]
    public class ResourceOwnerServiceTests
    {
        private readonly StartupTests _factory;

        public ResourceOwnerServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_FailClient()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = service.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Email,
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                var rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

                rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = string.Empty,
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                client.Enabled = false;

                await uow.ClientRepo.UpdateAsync(client);
                await uow.CommitAsync();

                rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_FailIssuer()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = service.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Email,
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                var rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

                rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = string.Empty,
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                issuer.Enabled = false;

                await uow.IssuerRepo.UpdateAsync(issuer);
                await uow.CommitAsync();

                rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_FailUser()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = service.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                /*
                 */
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

                await uow.UserRepo.UpdateAsync(user);
                await uow.CommitAsync();

                var rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                /*
                 */
                await uow.UserRepo.DeleteAsync(user.Id);
                await uow.CommitAsync();

                rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Refresh_FailToken()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var random = new Random();
                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                /*
                 */
                uow.ConfigRepo.ResourceOwnerRefreshFake = true;
                uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(1);

                var rop = service.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                uow.ConfigRepo.ResourceOwnerRefreshFake = false;
                uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

                var rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                /*
                 */
                uow.ConfigRepo.ResourceOwnerRefreshFake = true;
                uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(-1);

                rop = service.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                uow.ConfigRepo.ResourceOwnerRefreshFake = false;
                uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

                rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                /*
                 */
                var pos = random.Next((rop.refresh_token).Length - 8);

                rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = (rop.refresh_token).Remove(pos, 8).Insert(pos, RandomValues.CreateBase64String(8)),
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

                /*
                 */
                rt = await service.Http.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = string.Empty,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_FailClient()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                uow.ConfigRepo.LegacyModeIssuer = false;

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await service.Http.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

                rop = await service.Http.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = string.Empty,
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                client.Enabled = false;

                await uow.ClientRepo.UpdateAsync(client);
                await uow.CommitAsync();

                rop = await service.Http.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_FailClient_Legacy()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                uow.ConfigRepo.LegacyModeIssuer = true;

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await service.Http.ResourceOwner_UseV1Legacy(
                    new ResourceOwnerV1()
                    {
                        client_id = Guid.NewGuid().ToString(),
                        username = user.Id.ToString(),
                        grant_type = "password",
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

                rop = await service.Http.ResourceOwner_UseV1Legacy(
                    new ResourceOwnerV1()
                    {
                        client_id = string.Empty,
                        username = user.Id.ToString(),
                        grant_type = "password",
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                client.Enabled = false;

                await uow.ClientRepo.UpdateAsync(client);
                await uow.CommitAsync();

                rop = await service.Http.ResourceOwner_UseV1Legacy(
                    new ResourceOwnerV1()
                    {
                        client_id = client.Id.ToString(),
                        username = user.Id.ToString(),
                        grant_type = "password",
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_FailIssuer()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                uow.ConfigRepo.LegacyModeIssuer = false;

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await service.Http.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

                rop = await service.Http.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = string.Empty,
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                issuer.Enabled = false;

                await uow.IssuerRepo.UpdateAsync(issuer);
                await uow.CommitAsync();

                rop = await service.Http.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Email,
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_FailUser()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                uow.ConfigRepo.LegacyModeIssuer = false;

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await service.Http.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = Guid.NewGuid().ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

                rop = await service.Http.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = string.Empty,
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                rop = await service.Http.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = RandomValues.CreateBase64String(8),
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

                await uow.UserRepo.UpdateAsync(user);

                rop = await service.Http.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_FailUser_Legacy()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                uow.ConfigRepo.LegacyModeIssuer = true;

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await service.Http.ResourceOwner_UseV1Legacy(
                    new ResourceOwnerV1()
                    {
                        client_id = client.Id.ToString(),
                        username = Guid.NewGuid().ToString(),
                        grant_type = "password",
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

                rop = await service.Http.ResourceOwner_UseV1Legacy(
                    new ResourceOwnerV1()
                    {
                        client_id = client.Id.ToString(),
                        username = string.Empty,
                        grant_type = "password",
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                rop = await service.Http.ResourceOwner_UseV1Legacy(
                    new ResourceOwnerV1()
                    {
                        client_id = client.Id.ToString(),
                        username = user.Id.ToString(),
                        grant_type = "password",
                        password = RandomValues.CreateBase64String(8),
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.UtcNow.AddSeconds(60);

                await uow.UserRepo.UpdateAsync(user);

                rop = await service.Http.ResourceOwner_UseV1Legacy(
                    new ResourceOwnerV1()
                    {
                        client_id = client.Id.ToString(),
                        username = user.Id.ToString(),
                        grant_type = "password",
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV1_Use_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var salt = conf["IdentityTenants:Salt"];
                salt.Should().Be(uow.IssuerRepo.Salt);

                /*
                 */
                uow.ConfigRepo.LegacyModeIssuer = false;

                var rop = service.ResourceOwner_UseV1(
                    new ResourceOwnerV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Email,
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo<UserJwtV1>();

                JwtFactory.CanReadToken(rop.access_token).Should().BeTrue();

                var rop_claims = JwtFactory.ReadJwtToken(rop.access_token).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                rop_claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                rop_claims.Value.Split(':')[1].Should().Be(salt);

                var rt = service.ResourceOwnerRefresh_UseV1(
                    new RefreshTokenV1()
                    {
                        issuer_id = issuer.Id.ToString(),
                        client_id = client.Id.ToString(),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo<UserJwtV1>();

                JwtFactory.CanReadToken(rt.access_token).Should().BeTrue();

                var rt_claims = JwtFactory.ReadJwtToken(rt.access_token).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                rt_claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                rt_claims.Value.Split(':')[1].Should().Be(salt);

                /*
                 */
                uow.ConfigRepo.LegacyModeIssuer = true;

                var rop_legacy = service.ResourceOwner_UseV1Legacy(
                    new ResourceOwnerV1()
                    {
                        client_id = client.Id.ToString(),
                        grant_type = "password",
                        username = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop_legacy.Should().BeAssignableTo<UserJwtV1Legacy>();

                JwtFactory.CanReadToken(rop_legacy.access_token).Should().BeTrue();

                var rop_legacy_claims = JwtFactory.ReadJwtToken(rop_legacy.access_token).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                rop_legacy_claims.Value.Should().Be(Constants.ApiUnitTestIssuer);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_FailClient()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = service.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                var rt = await service.Http.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { Guid.NewGuid().ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

                client.Enabled = false;

                await uow.ClientRepo.UpdateAsync(client);
                await uow.CommitAsync();

                rt = await service.Http.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_FailIssuer()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = service.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                var rt = await service.Http.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

                rt = await service.Http.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = string.Empty,
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                issuer.Enabled = false;

                await uow.IssuerRepo.UpdateAsync(issuer);
                await uow.CommitAsync();

                rt = await service.Http.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_FailToken()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var random = new Random();
                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                /*
                 */
                uow.ConfigRepo.ResourceOwnerRefreshFake = true;
                uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(1);

                var rop = service.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                uow.ConfigRepo.ResourceOwnerRefreshFake = false;
                uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

                var rt = await service.Http.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                /*
                 */
                uow.ConfigRepo.ResourceOwnerRefreshFake = true;
                uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow.AddYears(-1);

                rop = service.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                uow.ConfigRepo.ResourceOwnerRefreshFake = false;
                uow.ConfigRepo.ResourceOwnerRefreshFakeUtcNow = DateTime.UtcNow;

                rt = await service.Http.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                /*
                 */
                rop = service.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                var rop_pos = random.Next((rop.refresh_token).Length - 8);

                rt = await service.Http.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = (rop.refresh_token).Remove(rop_pos, 8).Insert(rop_pos, RandomValues.CreateBase64String(8)),
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotFound);

                /*
                 */
                rop = service.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                rt = await service.Http.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = string.Empty,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Refresh_FailUser()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                uow.ConfigRepo.LegacyModeIssuer = false;

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = service.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

                await uow.UserRepo.UpdateAsync(user);
                await uow.CommitAsync();

                var rt = await service.Http.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = (string)rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await uow.UserRepo.DeleteAsync(user.Id);
                await uow.CommitAsync();

                rt = await service.Http.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = rop.refresh_token,
                    });
                rt.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rt.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Use_FailClient()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                uow.ConfigRepo.LegacyModeIssuer = false;

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await service.Http.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString(), RandomValues.CreateBase64String(8) }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

                client.Enabled = false;

                await uow.ClientRepo.UpdateAsync(client);
                await uow.CommitAsync();

                rop = await service.Http.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Use_FailIssuer()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                uow.ConfigRepo.LegacyModeIssuer = false;

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await service.Http.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

                rop = await service.Http.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = string.Empty,
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                issuer.Enabled = false;

                await uow.IssuerRepo.UpdateAsync(issuer);
                await uow.CommitAsync();

                rop = await service.Http.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Use_FailUser()
        {
            using (var owin = _factory.CreateClient())
            {
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                uow.ConfigRepo.LegacyModeIssuer = false;

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await service.Http.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = Guid.NewGuid().ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.NotFound);

                rop = await service.Http.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = string.Empty,
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                rop = await service.Http.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = RandomValues.CreateBase64String(8),
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(60);

                await uow.UserRepo.UpdateAsync(user);

                rop = await service.Http.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                rop.Should().BeAssignableTo(typeof(HttpResponseMessage));
                rop.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ResourceOwnerV2_Use_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsService(uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                uow.ConfigRepo.LegacyModeIssuer = false;

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var defaultClient = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var defaultRole = (await uow.RoleRepo.GetAsync(x => x.Name == Constants.ApiDefaultRoleForUser)).Single();

                var salt = conf["IdentityTenants:Salt"];
                salt.Should().Be(uow.IssuerRepo.Salt);

                if (client.Id == defaultClient.Id)
                    throw new ArgumentException();

                await uow.UserRepo.AddToRoleAsync(user, defaultRole);
                await uow.CommitAsync();

                /*
                 */
                var empty_rop = service.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { string.Empty }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                empty_rop.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(empty_rop.access_token).Should().BeTrue();

                var empty_rop_claims = JwtFactory.ReadJwtToken(empty_rop.access_token).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                empty_rop_claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                empty_rop_claims.Value.Split(':')[1].Should().Be(salt);

                var empty_rt = service.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { string.Empty }),
                        grant_type = "refresh_token",
                        refresh_token = empty_rop.refresh_token,
                    });
                empty_rt.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(empty_rt.access_token).Should().BeTrue();

                var empty_rt_claims = JwtFactory.ReadJwtToken(empty_rt.access_token).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                empty_rt_claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                empty_rt_claims.Value.Split(':')[1].Should().Be(salt);

                /*
                 */
                var single_rop = service.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Name }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                single_rop.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(single_rop.access_token).Should().BeTrue();

                var single_rop_claims = JwtFactory.ReadJwtToken(single_rop.access_token).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                single_rop_claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                single_rop_claims.Value.Split(':')[1].Should().Be(salt);

                var single_rt = service.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Name }),
                        grant_type = "refresh_token",
                        refresh_token = single_rop.refresh_token,
                    });
                single_rt.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(single_rt.access_token).Should().BeTrue();

                var single_rt_claims = JwtFactory.ReadJwtToken(single_rt.access_token).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                single_rt_claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                single_rt_claims.Value.Split(':')[1].Should().Be(salt);

                /*
                 */
                var multiple_rop = service.ResourceOwner_UseV2(
                    new ResourceOwnerV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString(), defaultClient.Id.ToString() }),
                        grant_type = "password",
                        user = user.Id.ToString(),
                        password = Constants.ApiUnitTestUserPassCurrent,
                    });
                multiple_rop.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(multiple_rop.access_token).Should().BeTrue();

                var multiple_rop_claims = JwtFactory.ReadJwtToken(multiple_rop.access_token).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                multiple_rop_claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                multiple_rop_claims.Value.Split(':')[1].Should().Be(salt);

                var multiple_rt = service.ResourceOwnerRefresh_UseV2(
                    new RefreshTokenV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Join(",", new List<string> { client.Id.ToString(), defaultClient.Id.ToString() }),
                        grant_type = "refresh_token",
                        refresh_token = multiple_rop.refresh_token,
                    });
                multiple_rt.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(multiple_rt.access_token).Should().BeTrue();

                var multiple_rt_claims = JwtFactory.ReadJwtToken(multiple_rt.access_token).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                multiple_rt_claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                multiple_rt_claims.Value.Split(':')[1].Should().Be(salt);
            }
        }
    }
}
