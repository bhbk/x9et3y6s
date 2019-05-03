using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Alert.Tests.ServiceTests
{
    public class TextServiceTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;
        private readonly HttpClient _owin;

        public TextServiceTests(StartupTests factory)
        {
            _factory = factory;
            _owin = _factory.CreateClient();
        }

        [Fact]
        public async Task Alert_TextV1_Enqueue_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AlertService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                /*
                 * check security...
                 */

                var result = await service.Http.Enqueue_TextV1(RandomValues.CreateBase64String(8), new TextCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                /*
                 * check model and/or action...
                 */

                result = await service.Http.Enqueue_TextV1(rop.RawData, new TextCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                result = await service.Http.Enqueue_TextV1(rop.RawData,
                    new TextCreate()
                    {
                        FromId = Guid.NewGuid(),
                        FromPhoneNumber = user.PhoneNumber,
                        ToId = testUser.Id,
                        ToPhoneNumber = testUser.PhoneNumber,
                        Body = Constants.ApiUnitTestTextBody,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                result = await service.Http.Enqueue_TextV1(rop.RawData,
                    new TextCreate()
                    {
                        FromId = user.Id,
                        FromPhoneNumber = testUser.PhoneNumber,
                        ToId = testUser.Id,
                        ToPhoneNumber = testUser.PhoneNumber,
                        Body = Constants.ApiUnitTestTextBody,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Alert_TextV1_Enqueue_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var service = new AlertService(uow.InstanceType, _owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var result = await service.Http.Enqueue_TextV1(rop.RawData,
                    new TextCreate()
                    {
                        FromId = user.Id,
                        FromPhoneNumber = user.PhoneNumber,
                        ToId = testUser.Id,
                        ToPhoneNumber = testUser.PhoneNumber,
                        Body = Constants.ApiUnitTestTextBody,
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
