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
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new LoginController(Context);
            var user = Context.UserMgmt.Store.Get().First();
            var login = new LoginCreate()
            {
                LoginProvider = BaseLib.Statics.ApiUnitTestLogin + EntrophyHelper.GenerateRandomBase64(4)
            };
            var add = await Context.LoginMgmt.CreateAsync(new LoginFactory<LoginCreate>(login).Devolve());
            var model = new UserLoginCreate()
            {
                UserId = user.Id,
                LoginId = add.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.LoginProvider,
                ProviderKey = BaseLib.Statics.ApiUnitTestLoginName,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.AddLoginToUser(model.LoginId, model.UserId, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_Login_Create_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new LoginController(Context);
            var user = Context.UserMgmt.Store.Get().First();
            var model = new LoginCreate()
            {
                LoginProvider = BaseLib.Statics.ApiUnitTestLogin
            };

            var result = await controller.CreateLogin(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.LoginProvider.Should().Be(model.LoginProvider);
        }

        [TestMethod]
        public async Task Api_Admin_Login_RemoveFromUser_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new LoginController(Context);
            var user = Context.UserMgmt.Store.Get().First();
            var login = new LoginFactory<LoginCreate>(
                new LoginCreate()
                {
                    LoginProvider = BaseLib.Statics.ApiUnitTestLogin + EntrophyHelper.GenerateRandomBase64(4)
                }).Devolve();
            var add = await Context.LoginMgmt.CreateAsync(login);
            var model = new UserLoginCreate()
            {
                UserId = user.Id,
                LoginId = add.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.LoginProvider,
                ProviderKey = BaseLib.Statics.ApiUnitTestLoginKey,
                Enabled = true,
                Immutable = false
            };

            var blah = await Context.UserMgmt.AddLoginAsync(user, 
                new UserLoginInfo(model.LoginProvider, model.ProviderKey, model.ProviderDisplayName));
            blah.Should().BeAssignableTo(typeof(IdentityResult));
            blah.Succeeded.Should().BeTrue();

            var result = await controller.RemoveLoginFromUser(model.LoginId, model.UserId) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_Login_Get_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new LoginController(Context);
            var login = Context.LoginMgmt.Store.Get().First();

            var result = await controller.GetLogin(login.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.Id.Should().Be(login.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Login_GetList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new LoginController(Context);

            var result = await controller.GetLogins() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<LoginResult>>().Subject;

            data.Count().Should().Equals(Context.LoginMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Login_GetUserList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new LoginController(Context);
            var login = Context.LoginMgmt.Store.Get().First();

            var result = await controller.GetLoginUsers(login.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<UserResult>>().Subject;

            data.Count().Should().Equals(Context.UserMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Login_Update_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new LoginController(Context);
            var login = Context.LoginMgmt.Store.Get().First();
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
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new LoginController(Context);
            var login = Context.LoginMgmt.Store.Get().First();

            var result = await controller.DeleteLogin(login.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = Context.LoginMgmt.Store.Get(x => x.Id == login.Id).Any();
            check.Should().BeFalse();
        }
    }
}
