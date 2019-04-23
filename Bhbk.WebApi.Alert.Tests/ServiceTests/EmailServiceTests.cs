using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Helpers;
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
        private readonly HttpClient _client;
        private readonly IAlertService _service;

        public EmailServiceTests(StartupTests factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _service = new AlertService(_factory.Conf, _factory.UoW.InstanceType, _client);
        }

        [Fact]
        public async Task Api_EmailV1_Enqueue_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var admin = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, admin);
            var result = await _service.Repo.Enqueue_EmailV1(rop.token, new EmailCreate());

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            result = await _service.Repo.Enqueue_EmailV1(rop.token,
                new EmailCreate()
                {
                    FromId = Guid.NewGuid(),
                    FromEmail = admin.Email,
                    ToId = user.Id,
                    ToEmail = user.Email,
                    Subject = Strings.ApiUnitTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                    HtmlContent = Strings.ApiUnitTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                });

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await _service.Repo.Enqueue_EmailV1(rop.token,
                new EmailCreate()
                {
                    FromId = admin.Id,
                    FromEmail = user.Email,
                    ToId = user.Id,
                    ToEmail = user.Email,
                    Subject = Strings.ApiUnitTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                    HtmlContent = Strings.ApiUnitTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                });

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Api_EmailV1_Enqueue_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();
            var user1 = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var rop = await JwtHelper.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

            var result = await _service.Repo.Enqueue_EmailV1(rop.token,
                new EmailCreate()
                {
                    FromId = user.Id,
                    FromEmail = user.Email,
                    ToId = user1.Id,
                    ToEmail = user1.Email,
                    Subject = Strings.ApiUnitTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                    HtmlContent = Strings.ApiUnitTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                });

            result.Should().BeAssignableTo(typeof(HttpResponseMessage));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
