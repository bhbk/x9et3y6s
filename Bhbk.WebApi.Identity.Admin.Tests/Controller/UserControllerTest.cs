﻿using Bhbk.Lib.Identity.Helper;
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

namespace Bhbk.WebApi.Identity.Admin.Tests.Controller
{
    [TestClass]
    public class UserControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public UserControllerTest()
        {
            _owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public async Task Api_Admin_User_Create_Success()
        {
            var controller = new UserController(UoW);
            var model = new UserModel.Binding.Create()
            {
                Email = BaseLib.Statics.ApiUnitTestsUserEmail + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net",
                FirstName = "FirstName",
                LastName = "LastName"
            };
            var result = await controller.CreateUser(model) as OkNegotiatedContentResult<UserModel.Return.User>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(UserModel.Return.User));
            result.Content.Email.Should().Be(model.Email);
        }

        [TestMethod]
        public async Task Api_Admin_User_Delete_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var result = await controller.DeleteUser(user.Id) as OkResult;
            var check = UoW.RoleRepository.Find(user.Id);

            result.Should().NotBeNull();
            check.Should().BeNull();
        }

        [TestMethod]
        public async Task Api_Admin_User_Get_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var result = await controller.GetUser(user.Id) as OkNegotiatedContentResult<UserModel.Return.User>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(UserModel.Return.User));
            result.Content.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public void Api_Admin_User_GetAll_Success()
        {
            var controller = new UserController(UoW);
            var result = controller.GetUsers() as OkNegotiatedContentResult<IEnumerable<UserModel.Return.User>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<UserModel.Return.User>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.UserRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_GetByName_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var result = await controller.GetUserByName(user.UserName) as OkNegotiatedContentResult<UserModel.Return.User>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(UserModel.Return.User));
            result.Content.Id.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task Api_Admin_User_GetProviderList_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var result = await controller.GetUserProviders(user.Id) as OkNegotiatedContentResult<IEnumerable<ProviderModel.Return.Provider>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<ProviderModel.Return.Provider>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.ProviderRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_GetRoleList_Success()
        {
            var controller = new UserController(UoW);
            var user = UoW.UserRepository.Get().First();
            var result = await controller.GetUserRoles(user.Id) as OkNegotiatedContentResult<IList<string>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IList<string>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.RoleRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_User_SetPassword_Fail()
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
        public async Task Api_Admin_User_SetPassword_Success()
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

        [TestMethod]
        public async Task Api_Admin_User_Update_Success()
        {
            string email = BaseLib.Statics.ApiUnitTestsUserEmail + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net";
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
                Immutable = false
            };
            var result = await controller.UpdateUser(model.Id, model) as OkNegotiatedContentResult<UserModel.Return.User>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(UserModel.Return.User));
            result.Content.Email.Should().Be(model.Email);
        }
    }
}
