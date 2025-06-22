using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Tests.RepositoryTests;
using Bhbk.Lib.Identity.Domain.Factories;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.WebApi.Identity.User.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.User.Tests.ControllerTests
{
    public class CredentialsControllerTests : IClassFixture<BaseControllerTests>
    {
        private readonly BaseControllerTests _factory;

        public CredentialsControllerTests(BaseControllerTests factory) => _factory = factory;

        #region Email Change Tests

        [Fact]
        public async ValueTask Me_CredentialsV1_ChangeEmail_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();
                var newEmail = Base64.CreateString(4) + "-" + _factory.TestData.User.UserName;

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new EmailChangeV1()
                {
                    EntityId = user.Id,
                    CurrentEmail = Base64.CreateString(4),
                    NewEmail = newEmail,
                    NewEmailConfirm = newEmail
                };

                var result = await controller.ChangeEmailV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public async ValueTask Me_CredentialsV1_ChangeEmail_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();
                var newEmail = Base64.CreateString(4) + "-" + _factory.TestData.User.UserName;

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new EmailChangeV1()
                {
                    EntityId = user.Id,
                    CurrentEmail = user.UserName,
                    NewEmail = newEmail,
                    NewEmailConfirm = newEmail
                };

                var result = await controller.ChangeEmailV1(model) as OkObjectResult;
                result.Should().BeAssignableTo<OkObjectResult>();
            }
        }

        #endregion

        #region Email Confirm Tests

        [Fact]
        public void Me_CredentialsV1_ConfirmEmail_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();
                var newEmail = string.Format("{0}{1}", Base64.CreateString(4), user.UserName);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var expire = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.GlobalTotpExpire).Single();

                var token = new PasswordTokenFactory(uow.InstanceType.ToString())
                    .Generate(newEmail, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user.Id.ToString(), user.SecurityStamp);
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmEmailV1(user.Id, newEmail,
                    Base64.CreateString(token.Length)) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public void Me_CredentialsV1_ConfirmEmail_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();
                var newEmail = string.Format("{0}{1}", Base64.CreateString(4), user.UserName);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var expire = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.GlobalTotpExpire).Single();

                var token = new PasswordTokenFactory(uow.InstanceType.ToString())
                    .Generate(newEmail, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user.Id.ToString(), user.SecurityStamp);
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmEmailV1(user.Id, newEmail, token) as NoContentResult;
                result.Should().BeAssignableTo<NoContentResult>();
            }
        }

        #endregion

        #region Password Change Tests

        [Fact]
        public async ValueTask Me_CredentialsV1_ChangePassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PasswordChangeV1()
                {
                    EntityId = user.Id,
                    CurrentPassword = Base64.CreateString(16),
                    NewPassword = _factory.TestData.User.PasswordNew,
                    NewPasswordConfirm = _factory.TestData.User.PasswordNew
                };

                var result = await controller.ChangePasswordV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public async ValueTask Me_CredentialsV1_ChangePassword_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PasswordChangeV1()
                {
                    EntityId = user.Id,
                    CurrentPassword = _factory.TestData.User.PasswordCurrent,
                    NewPassword = _factory.TestData.User.PasswordNew,
                    NewPasswordConfirm = _factory.TestData.User.PasswordNew
                };

                var result = await controller.ChangePasswordV1(model) as OkObjectResult;
                result.Should().BeAssignableTo<OkObjectResult>();
            }
        }

        #endregion

        #region Password Confirm Tests

        [Fact]
        public void Me_CredentialsV1_ConfirmPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();
                var newPassword = Base64.CreateString(12);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var expire = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.GlobalTotpExpire).Single();

                var token = new PasswordTokenFactory(uow.InstanceType.ToString())
                    .Generate(newPassword, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user.Id.ToString(), user.SecurityStamp);
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmPasswordV1(user.Id, newPassword,
                    Base64.CreateString(token.Length)) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public void Me_CredentialsV1_ConfirmPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();
                var newPassword = Base64.CreateString(12);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var expire = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == SettingsConstants.GlobalTotpExpire).Single();

                var token = new PasswordTokenFactory(uow.InstanceType.ToString())
                    .Generate(newPassword, TimeSpan.FromSeconds(uint.Parse(expire.ConfigValue)), user.Id.ToString(), user.SecurityStamp);
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmPasswordV1(user.Id, newPassword, token) as NoContentResult;
                result.Should().BeAssignableTo<NoContentResult>();
            }
        }

        #endregion

        #region Password Set Tests

        [Fact]
        public void Me_CredentialsV1_SetPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PasswordChangeV1()
                {
                    CurrentPassword = _factory.TestData.User.PasswordCurrent,
                    NewPassword = Base64.CreateString(16),
                    NewPasswordConfirm = Base64.CreateString(16)
                };

                var result = controller.SetPasswordV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public void Me_CredentialsV1_SetPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PasswordChangeV1()
                {
                    CurrentPassword = _factory.TestData.User.PasswordCurrent,
                    NewPassword = _factory.TestData.User.PasswordNew,
                    NewPasswordConfirm = _factory.TestData.User.PasswordNew
                };

                var result = controller.SetPasswordV1(model) as NoContentResult;
                result.Should().BeAssignableTo<NoContentResult>();
            }
        }

        #endregion

        #region Phone Change Tests

        [Fact]
        public async ValueTask Me_CredentialsV1_ChangePhone_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();
                var newPhone = NumberAs.CreateString(11);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PhoneChangeV1()
                {
                    EntityId = user.Id,
                    CurrentPhoneNumber = newPhone,
                    NewPhoneNumber = user.PhoneNumber,
                    NewPhoneNumberConfirm = user.PhoneNumber
                };

                var result = await controller.ChangePhoneV1(model) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public async ValueTask Me_CredentialsV1_ChangePhone_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();
                var newPhone = NumberAs.CreateString(11);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var model = new PhoneChangeV1()
                {
                    EntityId = user.Id,
                    CurrentPhoneNumber = user.PhoneNumber,
                    NewPhoneNumber = newPhone,
                    NewPhoneNumberConfirm = newPhone
                };

                var result = await controller.ChangePhoneV1(model) as OkObjectResult;
                result.Should().BeAssignableTo<OkObjectResult>();
            }
        }

        #endregion

        #region Phone Confirm Tests

        [Fact]
        public void Me_CredentialsV1_ConfirmPhone_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();
                var newPhoneNumber = NumberAs.CreateString(11);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var token = new TimeBasedTokenFactory(8, 10).Generate(newPhoneNumber, user.Id.ToString());
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmPhoneV1(user.Id, newPhoneNumber,
                    Base64.CreateString(token.Length)) as BadRequestObjectResult;
                result.Should().BeAssignableTo<BadRequestObjectResult>();
            }
        }

        [Fact]
        public void Me_CredentialsV1_ConfirmPhone_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var env = scope.ServiceProvider.GetRequiredService<IContextService>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var controller = new CredentialsController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    RequestServices = _factory.Server.Host.Services
                };

                var data = new TestDataFactory(uow, _factory.TestData);
                data.CreateAudiences();
                data.CreateUsers();

                var issuer = uow.Issuers.Get(x => x.Name == _factory.TestData.Issuer.Name).Single();
                var audience = uow.Audiences.Get(x => x.Name == _factory.TestData.Audience.Name).Single();
                var user = uow.Users.Get(x => x.UserName == _factory.TestData.User.UserName).Single();
                var newPhoneNumber = NumberAs.CreateString(11);

                controller.SetIdentity(issuer.Id, audience.Id, user.Id);

                var token = new TimeBasedTokenFactory(8, 10).Generate(newPhoneNumber, user.Id.ToString());
                token.Should().NotBeNullOrEmpty();

                var result = controller.ConfirmPhoneV1(user.Id, newPhoneNumber, token) as NoContentResult;
                result.Should().BeAssignableTo<NoContentResult>();
            }
        }

        #endregion
    }
}
