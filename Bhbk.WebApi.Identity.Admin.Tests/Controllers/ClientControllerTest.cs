using Bhbk.Lib.Identity.Factory;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
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
    public class ClientControllerTest : StartupTest
    {
        private TestServer _owin;

        public ClientControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_Client_Create_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            string name = BaseLib.Statics.ApiUnitTestClient + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new ClientController(Context);
            var model = new ClientCreate()
            {
                Name = name,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.CreateClient(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientModel>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Client_Delete_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new ClientController(Context);
            var client = Context.ClientMgmt.Store.Get().First();

            var result = await controller.DeleteClient(client.Id) as OkResult;
            result.Should().BeAssignableTo(typeof(OkResult));

            var check = Context.ClientMgmt.Store.Get(x => x.Id == client.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Client_Get_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new ClientController(Context);
            var client = Context.ClientMgmt.Store.Get().First();

            var result = await controller.GetClient(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientModel>().Subject;

            data.Id.Should().Be(client.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Client_GetList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new ClientController(Context);

            var result = await controller.GetClients() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<ClientModel>>().Subject;

            data.Count().Should().Equals(Context.ClientMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Client_GetAudienceList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new ClientController(Context);
            var client = Context.ClientMgmt.Store.Get().First();

            var result = await controller.GetClientAudiences(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<AudienceModel>>().Subject;

            data.Count().Should().Equals(Context.AudienceMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Client_Update_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            string name = BaseLib.Statics.ApiUnitTestClient + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new ClientController(Context);
            var client = Context.ClientMgmt.Store.Get().First();
            var model = new ClientUpdate()
            {
                Id = client.Id,
                Name = name + "(Updated)",
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateClient(model.Id, model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientModel>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
