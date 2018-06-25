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
        public async Task Api_Admin_Audience_Create_Fail_AudienceType()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new AudienceController(TestIoC, TestTasks);

            var model = new AudienceCreate()
            {
                ClientId = TestIoC.ClientMgmt.Store.Get().First().Id,
                Name = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestAudienceA,
                AudienceType = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(8),
                Enabled = true,
            };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.CreateAudience(model) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Delete_Fail_Immutable()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new AudienceController(TestIoC, TestTasks);

            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestIoC.AudienceMgmt.Store.SetImmutableAsync(audience, true);
            TestController.SetUser(user.Id);

            var result = await TestController.DeleteAudience(audience.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = TestIoC.AudienceMgmt.Store.Get(x => x.Id == audience.Id).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Create_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new AudienceController(TestIoC, TestTasks);

            var model = new AudienceCreate()
            {
                ClientId = TestIoC.ClientMgmt.Store.Get().First().Id,
                Name = BaseLib.Helpers.CryptoHelper.GenerateRandomBase64(4) + "-" + BaseLib.Statics.ApiUnitTestAudienceA,
                AudienceType = BaseLib.AudienceType.user_agent.ToString(),
                Enabled = true,
            };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.CreateAudience(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Name.Should().Be(model.Name);
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Delete_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new AudienceController(TestIoC, TestTasks);

            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.DeleteAudience(audience.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = TestIoC.AudienceMgmt.Store.Get(x => x.Id == audience.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Get_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new AudienceController(TestIoC, TestTasks);

            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.GetAudience(audience.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Id.Should().Be(audience.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Audience_GetList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();
            TestData.CreateTestDataRandom();

            ushort size = 3;
            var TestController = new AudienceController(TestIoC, TestTasks);
            var filter = new UrlFilter(size, 1, "name", "ascending");

            var result = await TestController.GetAudiences(filter) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<AudienceResult>>().Subject;

            data.Count().Should().Be(size);
        }

        [TestMethod]
        public async Task Api_Admin_Audience_GetRoleList_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new AudienceController(TestIoC, TestTasks);

            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();

            var result = await TestController.GetAudienceRoles(audience.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<RoleResult>>().Subject;

            data.Count().Should().Be(TestIoC.AudienceMgmt.Store.GetRoles(audience.Id).Count());
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Update_Success()
        {
            TestData.Destroy();
            TestData.CreateTestData();

            var TestController = new AudienceController(TestIoC, TestTasks);

            var audience = TestIoC.AudienceMgmt.Store.Get(x => x.Name == BaseLib.Statics.ApiUnitTestAudienceA).Single();
            var model = new AudienceUpdate()
            {
                Id = audience.Id,
                ClientId = TestIoC.ClientMgmt.Store.Get().First().Id,
                Name = BaseLib.Statics.ApiUnitTestAudienceA + "(Updated)",
                AudienceType = audience.AudienceType,
                Enabled = true
            };
            var user = TestIoC.UserMgmt.Store.Get(x => x.Email == BaseLib.Statics.ApiUnitTestUserA).Single();

            TestController.SetUser(user.Id);

            var result = await TestController.UpdateAudience(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Name.Should().Be(model.Name);
        }
    }
}
