using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests
{
    [TestClass]
    public class RealmControllerTest : BaseControllerTest
    {
        [TestMethod]
        public void Api_Realm_GetAll_Success()
        {
            var controller = new RealmController(UoW);
            var result = controller.GetRealms() as OkNegotiatedContentResult<IEnumerable<RealmModel.Return.Realm>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<RealmModel.Return.Realm>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.RealmRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Realm_Get_Success()
        {
            var controller = new RealmController(UoW);
            var realm = UoW.RealmRepository.Get().First();
            var result = await controller.GetRealm(realm.Id) as OkNegotiatedContentResult<RealmModel.Return.Realm>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(RealmModel.Return.Realm));
            result.Content.Id.Should().Be(realm.Id);
        }

        [TestMethod]
        public async Task Api_Realm_GetUserList_Success()
        {
            var controller = new RealmController(UoW);
            var realm = UoW.RealmRepository.Get().First();
            var result = await controller.GetUsersInRealm(realm.Id) as OkNegotiatedContentResult<IEnumerable<UserModel.Return.User>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<UserModel.Return.User>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.UserRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Realm_Create_Success()
        {
            string name = "Realm-UnitTest-" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new RealmController(UoW);
            var model = new RealmModel.Binding.Create()
            {
                Name = name,
                Enabled = true,
                Immutable = false
            };
            var result = await controller.CreateRealm(model) as OkNegotiatedContentResult<RealmModel.Return.Realm>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(RealmModel.Return.Realm));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Realm_Update_Success()
        {
            string name = "Realm-UnitTest-" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new RealmController(UoW);
            var realm = UoW.RealmRepository.Get().First();
            var model = new RealmModel.Binding.Update()
            {
                Id = realm.Id,
                Name = name + "(Updated)",
                Enabled = true,
                Immutable = false
            };
            var result = await controller.UpdateRealm(realm.Id, model) as OkNegotiatedContentResult<RealmModel.Return.Realm>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(RealmModel.Return.Realm));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Realm_Delete_Success()
        {
            var controller = new RealmController(UoW);
            var realm = UoW.RealmRepository.Get().First();
            var result = await controller.DeleteRealm(realm.Id) as OkResult;
            var check = UoW.RealmRepository.Find(realm.Id);

            result.Should().NotBeNull();
            check.Should().BeNull();
        }
    }
}
