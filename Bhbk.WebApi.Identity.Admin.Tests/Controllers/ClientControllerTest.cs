using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Infrastructure;
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

            var TestController = new ClientController(TestIoC, TestTasks);

            var model = new ClientCreate()
            {
                Name = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestClientA,
                ClientKey = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(32),
                Enabled = true,
            };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.CreateClient(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Client_Delete_Fail_Immutable()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new ClientController(TestIoC, TestTasks);

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestIoC.ClientMgmt.Store.SetImmutableAsync(client, true);
            TestController.SetUser(user.Id);

            var result = await TestController.DeleteClient(client.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = TestIoC.ClientMgmt.Store.Get(x => x.Id == client.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_Client_Delete_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new ClientController(TestIoC, TestTasks);

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.DeleteClient(client.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = TestIoC.ClientMgmt.Store.Get(x => x.Id == client.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Client_Get_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new ClientController(TestIoC, TestTasks);

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();

            var result = await TestController.GetClient(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Id.Should().Be(client.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Client_GetList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();
            TestData.CreateTestDataRandom();

            var TestController = new ClientController(TestIoC, TestTasks);
            ushort size = 3;
            var filter = new CustomPagingModel("name", size, 1);

            var result = await TestController.GetClients(filter) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<ClientResult>>().Subject;

            data.Count().Should().Be(size);
        }

        [TestMethod]
        public async Task Api_Admin_Client_GetAudienceList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new ClientController(TestIoC, TestTasks);

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();

            var result = await TestController.GetClientAudiences(client.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<AudienceResult>>().Subject;

            data.Count().Should().Be(TestIoC.ClientMgmt.Store.GetAudiences(client.Id).Count());
        }

        [TestMethod]
        public async Task Api_Admin_Client_Update_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new ClientController(TestIoC, TestTasks);

            var client = TestIoC.ClientMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestClientA).Single();
            var model = new ClientUpdate()
            {
                Id = client.Id,
                Name = BaseLib.Statics.ApiUnitTestClientA + "(Updated)",
                ClientKey = client.ClientKey,
                Enabled = true,
                Immutable = false
            };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.UpdateClient(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<ClientResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
