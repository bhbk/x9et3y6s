using Bhbk.Lib.Identity.Factory;
using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controller
{
    [TestClass]
    public class ProviderControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public ProviderControllerTest()
        {
            _owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public async Task Api_Admin_Provider_AddToUser_Success()
        {
            var controller = new ProviderController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();
            var model = new ProviderCreate()
            {
                Name = BaseLib.Statics.ApiUnitTestProvider + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };
            var create = await UoW.ProviderMgmt.CreateAsync(UoW.ProviderMgmt.Store.Mf.Create.DoIt(model));
            create.Should().BeAssignableTo(typeof(ProviderModel));

            var provider = await UoW.ProviderMgmt.FindByNameAsync(model.Name);
            provider.Should().BeAssignableTo(typeof(ProviderModel));

            var result = await controller.AddProviderToUser(provider.Id, user.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Admin_Provider_Create_Success()
        {
            string name = BaseLib.Statics.ApiUnitTestProvider + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new ProviderController(UoW);
            var model = new ProviderCreate()
            {
                Name = name,
                Enabled = true,
                Immutable = false
            };
            var result = await controller.CreateProvider(model) as OkNegotiatedContentResult<ProviderModel>;
            result.Content.Should().BeAssignableTo(typeof(ProviderModel));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Provider_RemoveFromUser_Success()
        {
            var controller = new ProviderController(UoW);
            var user = UoW.UserMgmt.Store.Get().First();
            var model = new ProviderCreate()
            {
                Name = BaseLib.Statics.ApiUnitTestProvider + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };
            var create = await UoW.ProviderMgmt.CreateAsync(UoW.ProviderMgmt.Store.Mf.Create.DoIt(model));
            create.Should().BeAssignableTo(typeof(ProviderModel));

            var provider = await UoW.ProviderMgmt.FindByNameAsync(model.Name);
            provider.Should().BeAssignableTo(typeof(ProviderModel));

            var add = await UoW.UserMgmt.AddToProviderAsync(user.Id, provider.Name);
            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            var result = await controller.RemoveProviderFromUser(provider.Id, user.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));
        }

        [TestMethod]
        public async Task Api_Admin_Provider_Get_Success()
        {
            var controller = new ProviderController(UoW);
            var provider = UoW.ProviderMgmt.Store.Get().First();

            var result = await controller.GetProvider(provider.Id) as OkNegotiatedContentResult<ProviderModel>;
            result.Content.Should().BeAssignableTo(typeof(ProviderModel));
            result.Content.Id.Should().Be(provider.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Provider_GetList_Success()
        {
            var controller = new ProviderController(UoW);

            var result = await controller.GetProviders() as OkNegotiatedContentResult<IList<ProviderModel>>;
            result.Content.Should().BeAssignableTo(typeof(IList<ProviderModel>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.ProviderMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Provider_GetUserList_Success()
        {
            var controller = new ProviderController(UoW);
            var provider = UoW.ProviderMgmt.Store.Get().First();

            var result = await controller.GetProviderUsers(provider.Id) as OkNegotiatedContentResult<IList<UserModel>>;
            result.Content.Should().BeAssignableTo(typeof(IList<UserModel>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.UserMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Provider_Update_Success()
        {
            string name = BaseLib.Statics.ApiUnitTestProvider + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new ProviderController(UoW);
            var provider = UoW.ProviderMgmt.Store.Get().First();
            var model = new ProviderUpdate()
            {
                Id = provider.Id,
                Name = name + "(Updated)",
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateProvider(provider.Id, model) as OkNegotiatedContentResult<ProviderModel>;
            result.Content.Should().BeAssignableTo(typeof(ProviderModel));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Provider_Delete_Success()
        {
            var controller = new ProviderController(UoW);
            var provider = UoW.ProviderMgmt.Store.Get().First();

            var result = await controller.DeleteProvider(provider.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = UoW.ProviderMgmt.Store.Get(x => x.Id == provider.Id).Any();
            check.Should().BeFalse();
        }
    }
}
