﻿using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Helpers;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
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
using FakeConstants = Bhbk.Lib.Identity.Domain.Tests.Primitives.Constants;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Alert.Tests.ServiceTests
{
    public class EmailServiceTests : IClassFixture<StartupTests>
    {
        private readonly StartupTests _factory;
        private readonly AlertService _service;

        public EmailServiceTests(StartupTests factory)
        {
            _factory = factory;

            var http = _factory.CreateClient();
            var conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();

            _service = new AlertService(conf, InstanceContext.UnitTest, http);
        }

        [Fact]
        public async Task Alert_EmailV1_Enqueue_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var result = await _service.Http.Enqueue_EmailV1(RandomValues.CreateBase64String(8), new EmailCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var result = await _service.Http.Enqueue_EmailV1(rop.RawData, new EmailCreate());
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
                var result = await _service.Http.Enqueue_EmailV1(rop.RawData,
                    new EmailCreate()
                    {
                        FromId = Guid.NewGuid(),
                        FromEmail = user.Email,
                        ToId = testUser.Id,
                        ToEmail = testUser.Email,
                        Subject = FakeConstants.ApiTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                        HtmlContent = FakeConstants.ApiTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
                var result = await _service.Http.Enqueue_EmailV1(rop.RawData,
                    new EmailCreate()
                    {
                        FromId = user.Id,
                        FromEmail = testUser.Email,
                        ToId = testUser.Id,
                        ToEmail = testUser.Email,
                        Subject = FakeConstants.ApiTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                        HtmlContent = FakeConstants.ApiTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Alert_EmailV1_Enqueue_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                new TestData(uow).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var testUser = (await uow.UserRepo.GetAsync(x => x.Email == FakeConstants.ApiTestUser)).Single();
                var result = await _service.Http.Enqueue_EmailV1(rop.RawData,
                    new EmailCreate()
                    {
                        FromId = user.Id,
                        FromEmail = user.Email,
                        ToId = testUser.Id,
                        ToEmail = testUser.Email,
                        Subject = FakeConstants.ApiTestEmailSubject + "-" + RandomValues.CreateBase64String(4),
                        HtmlContent = FakeConstants.ApiTestEmailContent + "-" + RandomValues.CreateBase64String(4)
                    });
                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
