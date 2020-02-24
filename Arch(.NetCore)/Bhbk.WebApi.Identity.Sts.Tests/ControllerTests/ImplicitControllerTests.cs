using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests;
using Bhbk.Lib.Identity.Data.EFCore.Services;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
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
using System.Web;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ControllerTests
{
    public class ImplicitControllerTests : IClassFixture<BaseControllerTests>
    {
        private readonly BaseControllerTests _factory;

        public ImplicitControllerTests(BaseControllerTests factory) => _factory = factory;

        [Fact]
        public void Sts_OAuth2_ImplicitV2_Auth_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var controller = new ImplicitController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.ApiTestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.ApiTestAudience).Single();
                var user = uow.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == Constants.ApiSettingAccessExpire).Single();

                var url = new Uri(Constants.ApiTestUriLink);

                var state = uow.States.Get(x => x.IssuerId == issuer.Id && x.AudienceId == audience.Id && x.UserId == user.Id
                    && x.StateType == StateType.User.ToString() && x.StateConsume == false
                    && x.ValidToUtc > DateTime.UtcNow).First();

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
                iss.Value.Split(':')[0].Should().Be(Constants.ApiTestIssuer);
                iss.Value.Split(':')[1].Should().Be(conf["IdentityTenants:Salt"]);

                var exp = Math.Round(DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwt.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Exp).SingleOrDefault().Value))
                    .Subtract(DateTime.UtcNow).TotalSeconds);
                exp.Should().BeInRange(uint.Parse(expire.ConfigValue) - 1, uint.Parse(expire.ConfigValue));
            }
        }
    }
}
