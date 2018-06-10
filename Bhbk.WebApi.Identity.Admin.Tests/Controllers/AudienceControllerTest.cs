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
    public class AudienceControllerTest : StartupTest
    {
        private TestServer _owin;

        public AudienceControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Create_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new AudienceController(TestIoC, TestTasks);
            var model = new AudienceCreate()
            {
                ClientId = TestIoC.ClientMgmt.Store.Get().First().Id,
                Name = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestAudienceA,
                AudienceType = BaseLib.AudienceType.user_agent.ToString(),
                Enabled = true,
                Immutable = false
            };

            var result = await controller.CreateAudience(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Delete_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new AudienceController(TestIoC, TestTasks);
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();

            var result = await controller.DeleteAudience(audience.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = TestIoC.AudienceMgmt.Store.Get(x => x.Id == audience.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Get_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new AudienceController(TestIoC, TestTasks);
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();

            var result = await controller.GetAudience(audience.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Id.Should().Be(audience.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Audience_GetList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new AudienceController(TestIoC, TestTasks);

            var result = await controller.GetAudiences() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<AudienceResult>>().Subject;

            data.Count().Should().Equals(TestIoC.AudienceMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Audience_GetRoleList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new AudienceController(TestIoC, TestTasks);
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();

            var result = await controller.GetAudienceRoles(audience.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<RoleResult>>().Subject;

            data.Count().Should().Equals(TestIoC.RoleMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Update_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var controller = new AudienceController(TestIoC, TestTasks);
            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var model = new AudienceUpdate()
            {
                Id = audience.Id,
                ClientId = TestIoC.ClientMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestAudienceA + "(Updated)",
                AudienceType = audience.AudienceType,
                Enabled = true
            };

            var result = await controller.UpdateAudience(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Name.Should().Equals(model.Name);
        }
    }
}
