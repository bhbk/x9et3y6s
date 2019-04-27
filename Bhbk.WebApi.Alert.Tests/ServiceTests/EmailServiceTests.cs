using Bhbk.Lib.Core.Cryptography;
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
    public class EmailServiceTests
    {
        private readonly StartupTests _factory;

        public EmailServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Alert_EmailV1_Enqueue_Fail()
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
                var result = await service.HttpClient.Enqueue_EmailV1(rop.token, new EmailCreate());

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                result = await service.HttpClient.Enqueue_EmailV1(rop.token,
                    new EmailCreate()
                    {
                        FromId = Guid.NewGuid(),
                        FromEmail = adminUser.Email,
                        ToId = normalUser.Id,
                        ToEmail = normalUser.Email,
                        Subject = Strings.ApiUnitTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                        HtmlContent = Strings.ApiUnitTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                    });

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                result = await service.HttpClient.Enqueue_EmailV1(rop.token,
                    new EmailCreate()
                    {
                        FromId = adminUser.Id,
                        FromEmail = normalUser.Email,
                        ToId = normalUser.Id,
                        ToEmail = normalUser.Email,
                        Subject = Strings.ApiUnitTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                        HtmlContent = Strings.ApiUnitTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                    });

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Alert_EmailV1_Enqueue_Success()
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

                var result = await service.HttpClient.Enqueue_EmailV1(rop.token,
                    new EmailCreate()
                    {
                        FromId = adminUser.Id,
                        FromEmail = adminUser.Email,
                        ToId = normalUser.Id,
                        ToEmail = normalUser.Email,
                        Subject = Strings.ApiUnitTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                        HtmlContent = Strings.ApiUnitTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                    });

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);

                await _factory.TestData.DestroyAsync();
            }
        }
    }
}
