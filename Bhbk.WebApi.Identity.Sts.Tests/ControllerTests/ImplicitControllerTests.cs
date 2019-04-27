using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ControllerTests
{
    [Collection("StsTests")]
    public class ImplicitControllerTests
    {
        private readonly StartupTests _factory;

        public ImplicitControllerTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Use_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new ImplicitController();
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

                var salt = conf["IdentityTenants:Salt"];
                salt.Should().Be(uow.IssuerRepo.Salt);

                var url = new Uri(Constants.ApiUnitTestUriLink);

                var state = await uow.StateRepo.CreateAsync(
                    new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.User.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.ImplicitTokenExpire),
                    });

                await uow.CommitAsync();

                var imp = await controller.ImplicitV2_Use(
                    new ImplicitV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state.StateValue,
                    }) as RedirectResult;
                imp.Should().BeAssignableTo(typeof(RedirectResult));
                imp.Permanent.Should().BeTrue();

                var imp_url = new Uri(imp.Url);
                var imp_uri = imp_url.AbsoluteUri.Substring(0, imp_url.AbsoluteUri.IndexOf('#'));

                imp_uri.Should().BeEquivalentTo(url.AbsoluteUri);

                /*
                 * implicit flow requires redirect with fragment in url. since the query parser library will not 
                 * process a fragment, need to replace # with ? so can test values in redirect...
                 */
                var imp_frag = "?" + imp_url.Fragment.Substring(1, imp_url.Fragment.Length - 1);

                HttpUtility.ParseQueryString(imp_frag).Get("state").Should().BeEquivalentTo(state.StateValue);
                HttpUtility.ParseQueryString(imp_frag).Get("grant_type").Should().BeEquivalentTo("implicit");
                HttpUtility.ParseQueryString(imp_frag).Get("token_type").Should().BeEquivalentTo("bearer");

                var imp_rop = HttpUtility.ParseQueryString(imp_frag).Get("access_token");

                JwtFactory.CanReadToken(imp_rop).Should().BeTrue();

                var imp_claims = JwtFactory.ReadJwtToken(imp_rop).Claims
                    .Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
                imp_claims.Value.Split(':')[0].Should().Be(Constants.ApiUnitTestIssuer);
                imp_claims.Value.Split(':')[1].Should().Be(salt);
            }
        }
    }
}
