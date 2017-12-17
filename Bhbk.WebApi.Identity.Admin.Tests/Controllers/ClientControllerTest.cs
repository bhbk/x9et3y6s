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
            TestData.Destroy();
            TestData.CreateTestData();

            string name = BaseLib.Statics.ApiUnitTestClient + BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4);
            var controller = new ClientController(IoC);
            var model = new ClientCreate()
            {
                Name = name,
                Enabled = true,
                Immutable = false
            };

            var result = await controller.CreateClient(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Client_Delete_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new ClientController(IoC);
            var client = IoC.ClientMgmt.Store.Get().First();

            var result = await controller.DeleteClient(client.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = IoC.ClientMgmt.Store.Get(x => x.Id == client.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Client_Get_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new ClientController(IoC);
            var client = IoC.ClientMgmt.Store.Get().First();

            var result = await controller.GetClient(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Id.Should().Be(client.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Client_GetList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new ClientController(IoC);

            var result = await controller.GetClients() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<ClientResult>>().Subject;

            data.Count().Should().Equals(IoC.ClientMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Client_GetAudienceList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new ClientController(IoC);
            var client = IoC.ClientMgmt.Store.Get().First();

            var result = await controller.GetClientAudiences(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<AudienceResult>>().Subject;

            data.Count().Should().Equals(IoC.AudienceMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Client_Update_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            string name = BaseLib.Statics.ApiUnitTestClient + BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4);
            var controller = new ClientController(IoC);
            var client = IoC.ClientMgmt.Store.Get().First();
            var model = new ClientUpdate()
            {
                Id = client.Id,
                Name = name + "(Updated)",
                Enabled = true,
                Immutable = false
            };

            var result = await controller.UpdateClient(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
