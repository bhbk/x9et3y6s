using Bhbk.Lib.Identity.Factory;
using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
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
    public class ClientControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public ClientControllerTest()
        {
            _owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public async Task Api_Admin_Client_Create_Success()
        {
            string name = BaseLib.Statics.ApiUnitTestClient + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new ClientController(UoW);
            var model = new ClientCreate()
            {
                Name = name,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.CreateClient(model) as OkNegotiatedContentResult<ClientModel>;
            result.Content.Should().BeAssignableTo(typeof(ClientModel));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Client_Delete_Success()
        {
            var controller = new ClientController(UoW);
            var client = UoW.ClientMgmt.Store.Get().First();

            var result = await controller.DeleteClient(client.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = UoW.ClientMgmt.Store.Get(x => x.Id == client.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Client_Get_Success()
        {
            var controller = new ClientController(UoW);
            var client = UoW.ClientMgmt.Store.Get().First();

            var result = await controller.GetClient(client.Id) as OkNegotiatedContentResult<ClientModel>;
            result.Content.Should().BeAssignableTo(typeof(ClientModel));
            result.Content.Id.Should().Be(client.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Client_GetList_Success()
        {
            var controller = new ClientController(UoW);

            var result = await controller.GetClients() as OkNegotiatedContentResult<IList<ClientModel>>;
            result.Content.Should().BeAssignableTo(typeof(IList<ClientModel>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.ClientMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Client_GetAudienceList_Success()
        {
            var controller = new ClientController(UoW);
            var client = UoW.ClientMgmt.Store.Get().First();

            var result = await controller.GetClientAudiences(client.Id) as OkNegotiatedContentResult<IList<AudienceModel>>;
            result.Content.Should().BeAssignableTo(typeof(IList<AudienceModel>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.AudienceMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Client_Update_Success()
        {
            string name = BaseLib.Statics.ApiUnitTestClient + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new ClientController(UoW);
            var client = UoW.ClientMgmt.Store.Get().First();
            var model = new ClientUpdate()
            {
                Id = client.Id,
                Name = name + "(Updated)",
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateClient(model.Id, model) as OkNegotiatedContentResult<ClientModel>;
            result.Content.Should().BeAssignableTo(typeof(ClientModel));
            result.Content.Name.Should().Be(model.Name);
        }
    }
}
