using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests
{
    [TestClass]
    public class UserControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public UserControllerTest()
        {
            //_owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public void Api_User_GetAll_Success()
        {
            var controller = new UserController(UoW);
            var result = controller.GetUsers() as OkNegotiatedContentResult<IEnumerable<UserModel.Return.User>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<UserModel.Return.User>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.UserRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_User_Get_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var result = await controller.GetUser(user.Id) as OkNegotiatedContentResult<UserModel.Return.User>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(UserModel.Return.User));
            result.Content.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_User_GetByName_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var result = await controller.GetUserByName(user.UserName) as OkNegotiatedContentResult<UserModel.Return.User>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(UserModel.Return.User));
            result.Content.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_User_GetRoleList_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var result = await controller.GetRolesForUser(user.Id) as OkNegotiatedContentResult<IList<string>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IList<string>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.RoleRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_User_Create_Success()
        {
            var controller = new UserController(UoW);
            var realm = UoW.RealmRepository.Get().First();
            var model = new UserModel.Binding.Create()
            {
                Email = "unit-test@" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net",
                FirstName = "FirstName",
                LastName = "LastName",
                RealmId = realm.Id
            };
            var result = await controller.CreateUser(model) as OkNegotiatedContentResult<UserModel.Return.User>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(UserModel.Return.User));
            result.Content.Email.Should().Be(model.Email);
        }

        [TestMethod]
        public async Task Api_User_Update_Success()
        {
            string email = "unit-test@" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var model = new UserModel.Binding.Update()
            {
                Id = user.Id,
                Email = user.Email,
                EmailConfirmed = true,
                FirstName = "FirstName",
                LastName = "LastName",
                LockoutEnabled = false,
                LockoutEndDateUtc = DateTime.Now.AddDays(30),
                TwoFactorEnabled = false,
                Immutable = false,
                RealmId = UoW.RealmRepository.Get().First().Id
            };
            var result = await controller.UpdateUser(model.Id, model) as OkNegotiatedContentResult<UserModel.Return.User>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(UserModel.Return.User));
            result.Content.Email.Should().Be(model.Email);
        }

        [TestMethod]
        public async Task Api_User_Delete_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var result = await controller.DeleteUser(user.Id) as OkResult;
            var check = UoW.RoleRepository.Find(user.Id);

            result.Should().NotBeNull();
            check.Should().BeNull();
        }

        [TestMethod]
        public async Task Api_User_DeleteToken_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var audience = UoW.AudienceRepository.Get().First();
            var model = new AppUserToken()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ProtectedTicket = EntrophyHelper.GenerateRandomBase64(64),
                IssuedUtc = DateTime.Now,
                ExpiresUtc = DateTime.Now.AddDays(30),
                AudienceId = audience.Id
            };
            var add = await UoW.CustomUserManager.AddRefreshTokenAsync(model);
            var result = await controller.DeleteToken(user.Id, audience.Id) as OkResult;
            var check = await UoW.CustomUserManager.FindRefreshTokenAsync(model.Id.ToString());

            result.Should().NotBeNull();
            check.Should().BeNull();
        }

        [TestMethod]
        public async Task Api_User_SetPassword_Fail()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var model = new UserModel.Binding.SetPassword()
            {
                NewPassword = EntrophyHelper.GenerateRandomBase64(16),
                NewPasswordConfirm = EntrophyHelper.GenerateRandomBase64(16)
            };
            var result = await controller.SetPassword(user.Id, model) as OkResult;
            var check = await UoW.CustomUserManager.CheckPasswordAsync(user, model.NewPassword);

            result.Should().BeNull();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_User_SetPassword_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var model = new UserModel.Binding.SetPassword()
            {
                NewPassword = BaseLib.Statics.ApiUnitTestsPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestsPasswordNew
            };
            var result = await controller.SetPassword(user.Id, model) as OkResult;
            var check = await UoW.CustomUserManager.CheckPasswordAsync(user, model.NewPassword);

            result.Should().NotBeNull();
            check.Should().BeTrue();
        }
    }
}
