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
            var create = Context.LoginMgmt.Store.Mf.Create.DoIt(login);
            var add = await Context.LoginMgmt.CreateAsync(create);
            var model = new UserLoginCreate()
            {
                UserId = user.Id,
                LoginId = add.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.LoginProvider,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.AddLoginToUser(model.LoginId, model.UserId, model) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
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
            var data = ok.Value.Should().BeAssignableTo<LoginModel>().Subject;

            data.LoginProvider.Should().Be(model.LoginProvider);
        }

        [TestMethod]
        public async Task Api_Admin_Login_RemoveFromUser_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new LoginController(Context);
            var user = Context.UserMgmt.Store.Get().First();
            var login = Context.LoginMgmt.Store.Mf.Create.DoIt(
                new LoginCreate()
                {
                    LoginProvider = BaseLib.Statics.ApiUnitTestLogin + EntrophyHelper.GenerateRandomBase64(4)
                });
            var add = await Context.LoginMgmt.CreateAsync(login);
            var model = new UserLoginCreate()
            {
                UserId = user.Id,
                LoginId = add.Id,
                LoginProvider = add.LoginProvider,
                ProviderDisplayName = add.LoginProvider,
                Enabled = true,
                Immutable = false
            };

            var blah = await Context.UserMgmt.AddLoginAsync(user.Id, Context.UserMgmt.Store.Mf.Create.DoIt(model));
            blah.Should().BeAssignableTo(typeof(IdentityResult));
            blah.Succeeded.Should().BeTrue();

            var result = await controller.RemoveLoginFromUser(model.LoginId, model.UserId) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
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
            var data = ok.Value.Should().BeAssignableTo<LoginModel>().Subject;

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
            var data = ok.Value.Should().BeAssignableTo<IList<LoginModel>>().Subject;

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
            var data = ok.Value.Should().BeAssignableTo<IList<UserModel>>().Subject;

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
                LoginProvider = BaseLib.Statics.ApiUnitTestLogin + "(Updated)"
            };

            var result = await controller.UpdateLogin(login.Id, model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginModel>().Subject;

            data.LoginProvider.Should().Be(model.LoginProvider);
        }

        [TestMethod]
        public async Task Api_Admin_Login_Delete_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new LoginController(Context);
            var login = Context.LoginMgmt.Store.Get().First();

            var result = await controller.DeleteLogin(login.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = Context.LoginMgmt.Store.Get(x => x.Id == login.Id).Any();
            check.Should().BeFalse();
        }
    }
}
