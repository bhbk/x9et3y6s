using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Sts.Tests.ServiceTests
{
    [Collection("StsTests")]
    public class ImplicitServiceTests
    {
        private readonly StartupTests _factory;

        public ImplicitServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Sts_OAuth2_ImplicitV1_Use_NotImplemented()
        {
            using (var owin = _factory.CreateClient())
            {
                var service = new StsService(_factory.Conf, _factory.UoW.InstanceType, owin);

                var imp = await service.HttpClient.Implicit_UseV1(
                    new ImplicitV1()
                    {
                        issuer_id = Guid.NewGuid().ToString(),
                        client_id = Guid.NewGuid().ToString(),
                        username = Guid.NewGuid().ToString(),
                        redirect_uri = RandomValues.CreateBase64String(8),
                        response_type = "token",
                        scope = RandomValues.CreateBase64String(8),
                        state = RandomValues.CreateBase64String(8),
                    });
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
            }
        }

        [Fact]
        public async Task Sts_OAuth2_ImplicitV2_Use_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new StsService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var url = new Uri(Strings.ApiUnitTestUriLink);
                var state = RandomValues.CreateBase64String(8);
                var imp = await service.HttpClient.Implicit_UseV2(
                    new ImplicitV2()
                    {
                        issuer = Guid.NewGuid().ToString(),
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state,
                    });
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.NotFound);

                imp = await service.HttpClient.Implicit_UseV2(
                    new ImplicitV2()
                    {
                        issuer = string.Empty,
                        client = client.Id.ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state,
                    });
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                imp = await service.HttpClient.Implicit_UseV2(
                    new ImplicitV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = Guid.NewGuid().ToString(),
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state,
                    });
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.NotFound);

                imp = await service.HttpClient.Implicit_UseV2(
                    new ImplicitV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Empty,
                        user = user.Id.ToString(),
                        redirect_uri = url.AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state,
                    });
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                imp = await service.HttpClient.Implicit_UseV2(
                    new ImplicitV2()
                    {
                        issuer = issuer.Id.ToString(),
                        client = string.Empty,
                        user = user.Id.ToString(),
                        redirect_uri = new Uri("https://app.test.net/a/invalid").AbsoluteUri,
                        response_type = "token",
                        scope = "any",
                        state = state,
                    });
                imp.Should().BeAssignableTo(typeof(HttpResponseMessage));
                imp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }
    }
}
