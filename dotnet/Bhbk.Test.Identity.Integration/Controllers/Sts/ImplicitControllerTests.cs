using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Test.Identity.Integration.TestingTools;
using Bhbk.WebApi.Identity.Sts.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Controllers.Sts
{
    public class ImplicitControllerTests : IClassFixture<BaseStsControllerTests>
    {
        private readonly BaseStsControllerTests _factory;

        public ImplicitControllerTests(BaseStsControllerTests factory) => _factory = factory;

        [Fact]
        public void Sts_OAuth2_ImplicitV2_Auth_Success()
        {
            using var owin = _factory.CreateClient();
            using var scope = _factory.Server.Host.Services.CreateScope();
            var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            using var seed = ScenarioMother.CreateOAuthImplicitSeed(uow);

            var controller = new ImplicitController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                RequestServices = _factory.Server.Host.Services
            };

            var issuer = seed.Issuer;
            var audience = seed.Audiences.Values.First();
            var user = seed.Users.Values.First();
            var state = seed.States.First();
            var urlEntity = seed.Urls.Values.First();
            var url = new Uri(urlEntity.UrlHost + urlEntity.UrlPath);

            var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.AccessExpire).Single();

            var imp = controller.ImplicitV2_Grant(
                new ImplicitV2()
                {
                    issuer = issuer.Id.ToString(),
                    client = audience.Id.ToString(),
                    grant_type = "implicit",
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
             * implicit flow requires redirect with fragment in url. since the query parser library will
             * not process a fragment, need to replace # with ? so can test values in redirect...
             */
            var imp_frag = "?" + imp_url.Fragment.Substring(1, imp_url.Fragment.Length - 1);

            HttpUtility.ParseQueryString(imp_frag).Get("state").Should().BeEquivalentTo(state.StateValue);
            HttpUtility.ParseQueryString(imp_frag).Get("grant_type").Should().BeEquivalentTo("implicit");
            HttpUtility.ParseQueryString(imp_frag).Get("token_type").Should().BeEquivalentTo("bearer");

            var result = HttpUtility.ParseQueryString(imp_frag).Get("access_token");

            auth.Valid(result).Should().BeTrue();

            var jwt = auth.Parse(result);

            var iss = jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Iss).SingleOrDefault();
            iss.Value.Split(':')[0].Should().Be(issuer.Name);
            iss.Value.Split(':')[1].Should().Be(conf["IdentityTenant:Salt"]);

            var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                .Subtract(DateTime.UtcNow).TotalSeconds);
            exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
        }
    }
}
