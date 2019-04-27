using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Repositories;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ControllerTests
{
    [Collection("StsTests")]
    public class AuthCodeControllerTests
    {
        private readonly StartupTests _factory;

        public AuthCodeControllerTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Ask_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new AuthCodeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var url = new Uri(Constants.ApiUnitTestUriLink);
                var ask = await controller.AuthCodeV2_Ask(
                    new AuthCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "code",
                        scope = "any",
                    }) as RedirectResult;
                ask.Should().NotBeNull();
                ask.Should().BeAssignableTo(typeof(RedirectResult));
                ask.Permanent.Should().BeTrue();

                var ask_url = new Uri(ask.Url);

                HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri").Should().BeEquivalentTo(Constants.ApiUnitTestUriLink);
                HttpUtility.ParseQueryString(ask_url.Query).Get("state").Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_Client()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new AuthCodeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsRepository(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var url = new Uri(Constants.ApiUnitTestUriLink);
                var ask = await controller.AuthCodeV2_Ask(
                    new AuthCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "code",
                        scope = "any",
                    }) as RedirectResult;
                ask.Should().NotBeNull();
                ask.Should().BeAssignableTo(typeof(RedirectResult));
                ask.Permanent.Should().BeTrue();

                var ask_url = new Uri(ask.Url);
                var ask_redirect = HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri");
                var ask_code = await new ProtectHelper(uow.InstanceType.ToString())
                    .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uow.ConfigRepo.AuthCodeTotpExpire), user);
                var ask_state = HttpUtility.ParseQueryString(ask_url.Query).Get("state");

                var ac = await service.AuthCode_UseV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        code = RandomValues.CreateBase64String(8),
                        state = ask_state,
                    });

                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                ac = await service.AuthCode_UseV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                        code = ask_code,
                        state = ask_state,
                    });

                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await uow.ClientRepo.DeleteAsync(client.Id);
                await uow.CommitAsync();

                ac = await service.AuthCode_UseV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        code = ask_code,
                        state = ask_state,
                    });

                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_Issuer()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new AuthCodeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsRepository(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var ask = await controller.AuthCodeV2_Ask(
                    new AuthCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = Constants.ApiUnitTestUriLink,
                        response_type = "code",
                        scope = "any",
                    }) as RedirectResult;
                ask.Should().NotBeNull();
                ask.Should().BeAssignableTo(typeof(RedirectResult));
                ask.Permanent.Should().BeTrue();

                var ask_url = new Uri(ask.Url);
                var ask_redirect = HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri");
                var ask_code = await new ProtectHelper(uow.InstanceType.ToString())
                    .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uow.ConfigRepo.AuthCodeTotpExpire), user);
                var ask_state = HttpUtility.ParseQueryString(ask_url.Query).Get("state");

                await uow.IssuerRepo.DeleteAsync(issuer.Id);
                await uow.CommitAsync();

                var ac = await service.AuthCode_UseV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = ask_redirect,
                        code = ask_code,
                        state = ask_state,
                    });

                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_User()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new AuthCodeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsRepository(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var url = new Uri(Constants.ApiUnitTestUriLink);
                var ask = await controller.AuthCodeV2_Ask(
                    new AuthCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "code",
                        scope = "any",
                    }) as RedirectResult;
                ask.Should().NotBeNull();
                ask.Should().BeAssignableTo(typeof(RedirectResult));
                ask.Permanent.Should().BeTrue();

                var ask_url = new Uri(ask.Url);
                var ask_redirect = HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri");
                var ask_code = await new ProtectHelper(uow.InstanceType.ToString())
                    .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uow.ConfigRepo.AuthCodeTotpExpire), user);
                var ask_state = HttpUtility.ParseQueryString(ask_url.Query).Get("state");

                var ac = await service.AuthCode_UseV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = Guid.NewGuid().ToString(),
                        redirect_uri = url.AbsoluteUri,
                        code = ask_code,
                        state = ask_state,
                    });

                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.NotFound);

                await uow.UserRepo.DeleteAsync(user.Id);
                await uow.CommitAsync();

                ac = await service.AuthCode_UseV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        code = ask_code,
                        state = ask_state,
                    });

                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Fail_Validate()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new AuthCodeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsRepository(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var url = new Uri(Constants.ApiUnitTestUriLink);
                var ask = await controller.AuthCodeV2_Ask(
                    new AuthCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "code",
                        scope = "any",
                    }) as RedirectResult;
                ask.Should().NotBeNull();
                ask.Should().BeAssignableTo(typeof(RedirectResult));
                ask.Permanent.Should().BeTrue();

                var ask_url = new Uri(ask.Url);
                var ask_redirect = HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri");
                var ask_code = await new ProtectHelper(uow.InstanceType.ToString())
                    .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uow.ConfigRepo.AuthCodeTotpExpire), user);
                var ask_state = HttpUtility.ParseQueryString(ask_url.Query).Get("state");

                var ac = await service.AuthCode_UseV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = ask_redirect,
                        code = ask_code,
                        state = RandomValues.CreateBase64String(32),
                    });

                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                ac = await service.AuthCode_UseV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = ask_redirect,
                        code = RandomValues.CreateBase64String(32),
                        state = ask_state,
                    });

                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                ac = await service.AuthCode_UseV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                        code = ask_code,
                        state = ask_state,
                    });

                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_AuthCodeV2_Use_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new AuthCodeController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new StsRepository(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var salt = conf["IdentityTenants:Salt"];
                salt.Should().Be(uow.IssuerRepo.Salt);

                var url = new Uri(Constants.ApiUnitTestUriLink);
                var ask = await controller.AuthCodeV2_Ask(
                    new AuthCodeAskV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "code",
                        scope = "any",
                    }) as RedirectResult;
                ask.Should().NotBeNull();
                ask.Should().BeAssignableTo(typeof(RedirectResult));
                ask.Permanent.Should().BeTrue();

                var ask_url = new Uri(ask.Url);
                var ask_redirect = HttpUtility.ParseQueryString(ask_url.Query).Get("redirect_uri");
                var ask_code = await new ProtectHelper(uow.InstanceType.ToString())
                    .GenerateAsync(user.SecurityStamp, TimeSpan.FromSeconds(uow.ConfigRepo.AuthCodeTotpExpire), user);
                var ask_state = HttpUtility.ParseQueryString(ask_url.Query).Get("state");

                var ac = await service.AuthCode_UseV2(
                    new AuthCodeV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = ask_redirect,
                        code = ask_code,
                        state = ask_state,
                    });

                ac.Should().BeAssignableTo(typeof(HttpResponseMessage));
                ac.StatusCode.Should().Be(HttpStatusCode.OK);

                var ac_ok = JObject.Parse(await ac.Content.ReadAsStringAsync());
                var ac_check = ac_ok.ToObject<UserJwtV2>();
                ac_check.Should().BeAssignableTo<UserJwtV2>();

                JwtFactory.CanReadToken(ac_check.access_token).Should().BeTrue();

                var ac_claims = JwtFactory.ReadJwtToken(ac_check.access_token).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                ac_claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                ac_claims.Value.Split(':')[1].Should().Be(salt);
            }
        }
    }
}
