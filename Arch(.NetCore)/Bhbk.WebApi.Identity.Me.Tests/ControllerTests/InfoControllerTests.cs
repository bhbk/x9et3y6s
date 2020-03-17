using AutoMapper;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Bhbk.WebApi.Identity.Me.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Xunit;

namespace Bhbk.WebApi.Identity.Me.Tests.ControllerTests
{
    public class InfoControllerTests : IClassFixture<BaseControllerTests>
    {
        private readonly BaseControllerTests _factory;

        public InfoControllerTests(BaseControllerTests factory) => _factory = factory;

        [Fact]
        public void Me_InfoV1_DeleteRefreshes_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new InfoController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var result = controller.DeleteRefreshV1(Guid.NewGuid()) as NotFoundObjectResult;
                result = controller.DeleteRefreshesV1() as NotFoundObjectResult;
            }
        }

        [Fact]
        public void Me_InfoV1_DeleteRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var auth = scope.ServiceProvider.GetRequiredService<IOAuth2JwtFactory>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new InfoController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                for (int i = 0; i < 3; i++)
                {
                    var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                    var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                    uow.Refreshes.Create(
                        mapper.Map<tbl_Refreshes>(new RefreshV1()
                        {
                            IssuerId = issuer.Id,
                            UserId = user.Id,
                            RefreshType = RefreshType.User.ToString(),
                            RefreshValue = rt.RawData,
                            ValidFromUtc = rt.ValidFrom,
                            ValidToUtc = rt.ValidTo,
                        }));
                }

                var refresh = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refreshes>()
                    .Where(x => x.UserId == user.Id).ToLambda()).First();

                var result = controller.DeleteRefreshV1(refresh.Id) as OkObjectResult;
                result = controller.DeleteRefreshesV1() as OkObjectResult;
            }
        }

        [Fact]
        public void Me_InfoV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new InfoController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var result = controller.GetV1() as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                ok.Value.Should().BeAssignableTo<UserV1>();
            }
        }

        [Fact]
        public void Me_InfoV1_GetRefreshes_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new InfoController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var result = controller.GetRefreshesV1() as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                ok.Value.Should().BeAssignableTo<IEnumerable<RefreshV1>>();
            }
        }

        [Fact]
        public void Me_InfoV1_SetPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new InfoController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PasswordChangeV1()
                {
                    CurrentPassword = Constants.TestUserPassCurrent,
                    NewPassword = Base64.CreateString(16),
                    NewPasswordConfirm = Base64.CreateString(16)
                };

                var result = controller.SetPasswordV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public void Me_InfoV1_SetPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new InfoController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PasswordChangeV1()
                {
                    CurrentPassword = Constants.TestUserPassCurrent,
                    NewPassword = Constants.TestUserPassNew,
                    NewPasswordConfirm = Constants.TestUserPassNew
                };

                var result = controller.SetPasswordV1(model) as NoContentResult;
                result.Should().BeAssignableTo<NoContentResult>();
            }
        }

        [Fact]
        public void Me_InfoV1_SetTwoFactor_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new InfoController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var result = controller.SetTwoFactorV1(true) as NoContentResult;
                result.Should().BeAssignableTo<NoContentResult>();
            }
        }

        [Fact]
        public void Me_InfoV1_Update_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var instance = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new InfoController(conf, instance);
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                new GenerateTestData(uow, mapper).Destroy();
                new GenerateTestData(uow, mapper).Create();

                var issuer = uow.Issuers.Get(x => x.Name == Constants.TestIssuer).Single();
                var audience = uow.Audiences.Get(x => x.Name == Constants.TestAudience).Single();
                var user = uow.Users.Get(x => x.UserName == Constants.TestUser).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                user.FirstName += "(Updated)";
                user.LastName += "(Updated)";

                var result = controller.UpdateV1(mapper.Map<UserV1>(user)) as OkObjectResult;
                var ok = result.Should().BeAssignableTo<OkObjectResult>().Subject;
                ok.Value.Should().BeAssignableTo<UserV1>();
            }
        }
    }
}
