using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
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
    [Collection("AlertTests")]
    public class EmailServiceTests
    {
        private readonly StartupTests _factory;

        public EmailServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Alert_EmailV1_Enqueue_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AlertService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.Enqueue_EmailV1(rop.token, new EmailCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                result = await service.Http.Enqueue_EmailV1(rop.token,
                    new EmailCreate()
                    {
                        FromId = Guid.NewGuid(),
                        FromEmail = user.Email,
                        ToId = testUser.Id,
                        ToEmail = testUser.Email,
                        Subject = Constants.ApiUnitTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                        HtmlContent = Constants.ApiUnitTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                result = await service.Http.Enqueue_EmailV1(rop.token,
                    new EmailCreate()
                    {
                        FromId = user.Id,
                        FromEmail = testUser.Email,
                        ToId = testUser.Id,
                        ToEmail = testUser.Email,
                        Subject = Constants.ApiUnitTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                        HtmlContent = Constants.ApiUnitTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Alert_EmailV1_Enqueue_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new AlertService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultAdminUser)).Single();

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await service.Http.Enqueue_EmailV1(rop.token,
                    new EmailCreate()
                    {
                        FromId = user.Id,
                        FromEmail = user.Email,
                        ToId = testUser.Id,
                        ToEmail = testUser.Email,
                        Subject = Constants.ApiUnitTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                        HtmlContent = Constants.ApiUnitTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
