using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [TestClass]
    public class LoginControllerTest : StartupTest
    {
        private TestServer _owin;

        public LoginControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_Login_AddToUser_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new LoginController(IoC);
            var user = IoC.UserMgmt.Store.Get().First();
            var login = new LoginCreate()
            {
                LoginProvider = BaseLib.Statics.ApiUnitTestLoginA + "-" + BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4)
            };
            var add = await IoC.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(login).Devolve());
            var model = new UserLoginCreate()
            {
                UserId = user.Id,
                LoginId = add.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.LoginProvider,
                ProviderKey = BaseLib.Statics.ApiUnitTestLoginKeyA,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.AddLoginToUser(model.LoginId, model.UserId, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_Login_Create_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new LoginController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var model = new LoginCreate()
            {
                LoginProvider = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestLoginA
            };

            var result = await controller.CreateLogin(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.LoginProvider.Should().Be(model.LoginProvider);
        }

        [TestMethod]
        public async Task Api_Admin_Login_RemoveFromUser_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new LoginController(IoC);
            var user = IoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();
            var login = new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestLoginA
                }).Devolve();
            var create = await IoC.LoginMgmt.CreateAsync(login);
            var model = new UserLoginCreate()
            {
                UserId = user.Id,
                LoginId = create.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.LoginProvider,
                ProviderKey = BaseLib.Statics.ApiUnitTestLoginKeyA,
                Enabled = true,
                Immutable = false
            };

            var add = await IoC.UserMgmt.AddLoginAsync(user, 
                new UserLoginInfo(model.LoginProvider, model.ProviderKey, model.ProviderDisplayName));
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            var result = await controller.RemoveLoginFromUser(model.LoginId, model.UserId) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_Login_Get_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new LoginController(IoC);
            var login = IoC.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();

            var result = await controller.GetLogin(login.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.Id.Should().Be(login.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Login_GetList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new LoginController(IoC);

            var result = await controller.GetLogins() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<LoginResult>>().Subject;

            data.Count().Should().Equals(IoC.LoginMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Login_GetUserList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new LoginController(IoC);
            var login = IoC.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();

            var result = await controller.GetLoginUsers(login.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<UserResult>>().Subject;
        }

        [TestMethod]
        public async Task Api_Admin_Login_Update_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new LoginController(IoC);
            var login = IoC.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();
            var model = new LoginUpdate()
            {
                Id = login.Id,
                LoginProvider = login.LoginProvider + "(Updated)"
            };

            var result = await controller.UpdateLogin(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.LoginProvider.Should().Be(model.LoginProvider);
        }

        [TestMethod]
        public async Task Api_Admin_Login_Delete_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new LoginController(IoC);
            var login = IoC.LoginMgmt.Store.Get(x => x.LoginProvider == BaseLib.Statics.ApiUnitTestLoginA).Single();

            var result = await controller.DeleteLogin(login.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = IoC.LoginMgmt.Store.Get(x => x.Id == login.Id).Any();
            check.Should().BeFalse();
        }
    }
}
