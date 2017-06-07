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
            var user = UoW.UserRepository.Get().First();
            var model = new AppProvider()
            {
                Id = Guid.NewGuid(),
                Name = BaseLib.Statics.ApiUnitTestsProvider + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4),
                Immutable = false
            };
            var provider = UoW.CustomProviderManager.CreateAsync(model);
            var result = await controller.AddProviderToUser(model.Id, user.Id) as OkResult;
            var check = user.Providers.Where(x => x.ProviderId == model.Id).Single();

            result.Should().NotBeNull();
            check.Should().NotBeNull();
        }

        [TestMethod]
        public async Task Api_Admin_Provider_Create_Success()
        {
            string name = BaseLib.Statics.ApiUnitTestsProvider + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new ProviderController(UoW);
            var model = new ProviderModel.Binding.Create()
            {
                Name = name,
                Enabled = true,
                Immutable = false
            };
            var result = await controller.CreateProvider(model) as OkNegotiatedContentResult<ProviderModel.Return.Provider>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(ProviderModel.Return.Provider));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Provider_RemoveFromUser_Success()
        {
            var controller = new ProviderController(UoW);
            var user = UoW.UserRepository.Get().First();
            var model = new AppProvider()
            {
                Id = Guid.NewGuid(),
                Name = BaseLib.Statics.ApiUnitTestsProvider + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4),
                Enabled = true,
                Immutable = false
            };
            var create = UoW.CustomProviderManager.CreateAsync(model);
            var add = UoW.CustomUserManager.AddToProviderAsync(user.Id, model.Name);
            var result = await controller.RemoveProviderFromUser(model.Id, user.Id) as OkResult;
            var check = user.Providers.Where(x => x.ProviderId == model.Id).SingleOrDefault();

            result.Should().NotBeNull();
            check.Should().BeNull();
        }

        [TestMethod]
        public async Task Api_Admin_Provider_Get_Success()
        {
            var controller = new ProviderController(UoW);
            var provider = UoW.ProviderRepository.Get().First();
            var result = await controller.GetProvider(provider.Id) as OkNegotiatedContentResult<ProviderModel.Return.Provider>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(ProviderModel.Return.Provider));
            result.Content.Id.Should().Be(provider.Id);
        }

        [TestMethod]
        public void Api_Admin_Provider_GetAll_Success()
        {
            var controller = new ProviderController(UoW);
            var result = controller.GetProviders() as OkNegotiatedContentResult<IEnumerable<ProviderModel.Return.Provider>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<ProviderModel.Return.Provider>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.ProviderRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Provider_GetUserList_Success()
        {
            var controller = new ProviderController(UoW);
            var provider = UoW.ProviderRepository.Get().First();
            var result = await controller.GetProviderUsers(provider.Id) as OkNegotiatedContentResult<IEnumerable<UserModel.Return.User>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<UserModel.Return.User>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.UserRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Provider_Update_Success()
        {
            string name = BaseLib.Statics.ApiUnitTestsProvider + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new ProviderController(UoW);
            var provider = UoW.ProviderRepository.Get().First();
            var model = new ProviderModel.Binding.Update()
            {
                Id = provider.Id,
                Name = name + "(Updated)",
                Enabled = true,
                Immutable = false
            };
            var result = await controller.UpdateProvider(provider.Id, model) as OkNegotiatedContentResult<ProviderModel.Return.Provider>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(ProviderModel.Return.Provider));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Provider_Delete_Success()
        {
            var controller = new ProviderController(UoW);
            var provider = UoW.ProviderRepository.Get().First();
            var result = await controller.DeleteProvider(provider.Id) as OkResult;
            var check = UoW.ProviderRepository.Find(provider.Id);

            result.Should().NotBeNull();
            check.Should().BeNull();
        }
    }
}
