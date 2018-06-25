using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [TestClass]
    public class ActivityControllerTest : StartupTest
    {
        private TestServer _owin;

        public ActivityControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_Activity_GetList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();
            TestData.CreateTestDataRandom();

            ushort size = 3;
            var controller = new ActivityController(TestIoC, TestTasks);
            var filter = new UrlFilter(size, 1, "created", "descending");

            var result = await controller.GetActivity(filter) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<ActivityResult>>().Subject;

            data.Count().Should().Be(size);
        }
    }
}
