using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.WebApi.Identity.Me.Controller;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Tests.Controller
{
    [TestClass]
    public class MeControllerTest : BaseControllerTest
    {
        [TestMethod]
        public async Task Api_Me_ChangePassword_Fail()
        {
            string email = "unit-test@" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new MeController(UoW);
            var user = UoW.UserRepository.Get().First();
            var model = new UserModel.Binding.ChangePassword()
            {
                CurrentPassword = EntrophyHelper.GenerateRandomBase64(16),
                NewPassword = BaseLib.Statics.ApiUnitTestsPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestsPasswordNew
            };
            var result = await controller.ChangePassword(user.Id, model) as OkResult;
            var check = await UoW.CustomUserManager.CheckPasswordAsync(user, model.NewPassword);

            result.Should().BeNull();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Me_ChangePassword_Success()
        {
            string email = "unit-test@" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4) + ".net";
            var controller = new MeController(UoW);
            var user = UoW.UserRepository.Get().First();
            var model = new UserModel.Binding.ChangePassword()
            {
                CurrentPassword = BaseLib.Statics.ApiUnitTestsPassword,
                NewPassword = BaseLib.Statics.ApiUnitTestsPasswordNew,
                NewPasswordConfirm = BaseLib.Statics.ApiUnitTestsPasswordNew
            };
            var result = await controller.ChangePassword(user.Id, model) as OkResult;
            var check = await UoW.CustomUserManager.CheckPasswordAsync(user, model.NewPassword);

            result.Should().NotBeNull();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Me_Update_Success()
        {
            var controller = new MeController(UoW);
            var user = UoW.UserRepository.Get().First();
            var model = new UserModel.Binding.Update()
            {
                Id = user.Id,
                FirstName = user.FirstName + "(Updated)",
                LastName = user.LastName + "(Updated)"
            };
            var result = await controller.UpdateMe(user.Id, model) as OkNegotiatedContentResult<UserModel.Return.User>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(UserModel.Return.User));
            result.Content.FirstName.ShouldBeEquivalentTo(UoW.UserRepository.Find(user.Id).FirstName);
            result.Content.LastName.ShouldBeEquivalentTo(UoW.UserRepository.Find(user.Id).LastName);
        }
    }
}
