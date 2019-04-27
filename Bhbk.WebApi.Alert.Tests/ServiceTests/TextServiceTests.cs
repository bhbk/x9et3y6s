using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Alert.Tests.ServiceTests
{
    [Collection("AlertTests")]
    public class TextServiceTests
    {
        private readonly StartupTests _factory;

        public TextServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Alert_TextV1_Enqueue_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AlertService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var adminUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();
                var normalUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, adminUser);
                var result = await service.Endpoints.Enqueue_TextV1(rop.token, new TextCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                result = await service.Endpoints.Enqueue_TextV1(rop.token,
                    new TextCreate()
                    {
                        FromId = Guid.NewGuid(),
                        FromPhoneNumber = adminUser.PhoneNumber,
                        ToId = normalUser.Id,
                        ToPhoneNumber = normalUser.PhoneNumber,
                        Body = Strings.ApiUnitTestTextBody,
                    });

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                result = await service.Endpoints.Enqueue_TextV1(rop.token,
                    new TextCreate()
                    {
                        FromId = adminUser.Id,
                        FromPhoneNumber = normalUser.PhoneNumber,
                        ToId = normalUser.Id,
                        ToPhoneNumber = normalUser.PhoneNumber,
                        Body = Strings.ApiUnitTestTextBody,
                    });

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Alert_TextV1_Enqueue_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                var service = new AlertService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var adminUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();
                var normalUser = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, adminUser);

                var result = await service.Endpoints.Enqueue_TextV1(rop.token,
                    new TextCreate()
                    {
                        FromId = adminUser.Id,
                        FromPhoneNumber = adminUser.PhoneNumber,
                        ToId = normalUser.Id,
                        ToPhoneNumber = normalUser.PhoneNumber,
                        Body = Strings.ApiUnitTestTextBody,
                    });

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                await _factory.TestData.DestroyAsync();
            }
        }
    }
}
