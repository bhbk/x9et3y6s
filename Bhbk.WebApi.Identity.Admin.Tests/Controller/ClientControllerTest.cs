using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.WebApi.Identity.Admin.Controller;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tests
{
    [TestClass]
    public class ClientControllerTest : BaseControllerTest
    {
        private TestServer _owin;

        public ClientControllerTest()
        {
            //_owin = TestServer.Create<BaseControllerTest>();
        }

        [TestMethod]
        public async Task Api_Client_Create_Success()
        {
            string name = "Client-UnitTest-" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new ClientController(UoW);
            var model = new ClientModel.Binding.Create()
            {
                Name = name,
                Enabled = true,
                Immutable = false
            };
            var result = await controller.CreateClient(model) as OkNegotiatedContentResult<ClientModel.Return.Client>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(ClientModel.Return.Client));
            result.Content.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Client_Delete_Success()
        {
            var controller = new ClientController(UoW);
            var client = UoW.ClientRepository.Get().First();

            var result = await controller.DeleteClient(client.Id) as OkResult;
            var check = UoW.RoleRepository.Find(client.Id);

            result.Should().NotBeNull();
            check.Should().BeNull();
        }

        [TestMethod]
        public async Task Api_Client_Get_Success()
        {
            var controller = new ClientController(UoW);
            var client = UoW.ClientRepository.Get().First();
            var result = await controller.GetClient(client.Id) as OkNegotiatedContentResult<ClientModel.Return.Client>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(ClientModel.Return.Client));
            result.Content.Id.Should().Be(client.Id);
        }

        [TestMethod]
        public void Api_Client_GetAll_Success()
        {
            var controller = new ClientController(UoW);
            var result = controller.GetClients() as OkNegotiatedContentResult<IEnumerable<ClientModel.Return.Client>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<ClientModel.Return.Client>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.ClientRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Client_GetAudienceList_Success()
        {
            var controller = new ClientController(UoW);
            var client = UoW.ClientRepository.Get().First();
            var result = await controller.GetAudiencesInClient(client.Id) as OkNegotiatedContentResult<IEnumerable<AudienceModel.Return.Audience>>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(IEnumerable<AudienceModel.Return.Audience>));
            result.Content.Count().ShouldBeEquivalentTo(UoW.AudienceRepository.Get().Count());
        }

        [TestMethod]
        public async Task Api_Client_Update_Success()
        {
            string name = "Client-UnitTest-" + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new ClientController(UoW);
            var client = UoW.ClientRepository.Get().First();
            var model = new ClientModel.Binding.Update()
            {
                Id = client.Id,
                Name = name + "(Updated)",
                Enabled = true,
                Immutable = false
            };
            var result = await controller.UpdateClient(model.Id, model) as OkNegotiatedContentResult<ClientModel.Return.Client>;

            result.Should().NotBeNull();
            result.Content.Should().BeAssignableTo(typeof(ClientModel.Return.Client));
            result.Content.Name.Should().Be(model.Name);
        }
    }
}
