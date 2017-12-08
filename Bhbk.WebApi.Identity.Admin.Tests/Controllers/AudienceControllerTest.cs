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
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            string name = BaseLib.Statics.ApiUnitTestAudience + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new AudienceController(Context);
            var model = new AudienceCreate()
            {
                ClientId = Context.ClientMgmt.Store.Get().First().Id,
                Name = name,
                AudienceType = BaseLib.AudienceType.thin_client.ToString(),
                AudienceKey = BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(32),
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
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new AudienceController(Context);
            var audience = Context.AudienceMgmt.Store.Get().First();

            var result = await controller.DeleteAudience(audience.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = Context.AudienceMgmt.Store.Get(x => x.Id == audience.Id).Any();
            check.Should().BeFalse();
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Get_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new AudienceController(Context);
            var audience = Context.AudienceMgmt.Store.Get().First();

            var result = await controller.GetAudience(audience.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Id.Should().Be(audience.Id);
        }

        [TestMethod]
        public async Task Api_Admin_Audience_GetList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new AudienceController(Context);

            var result = await controller.GetAudiences() as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<AudienceResult>>().Subject;

            data.Count().Should().Equals(Context.AudienceMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Audience_GetRoleList_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            var controller = new AudienceController(Context);
            var audience = Context.AudienceMgmt.Store.Get().First();

            var result = await controller.GetAudienceRoles(audience.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IList<RoleResult>>().Subject;

            data.Count().Should().Equals(Context.RoleMgmt.Store.Get().Count());
        }

        [TestMethod]
        public async Task Api_Admin_Audience_Update_Success()
        {
            TestData.CompleteDestroy();
            TestData.TestDataCreate();

            string name = BaseLib.Statics.ApiUnitTestAudience + BaseLib.Helpers.EntrophyHelper.GenerateRandomBase64(4);
            var controller = new AudienceController(Context);
            var audience = Context.AudienceMgmt.Store.Get().First();
            var model = new AudienceUpdate()
            {
                Id = audience.Id,
                ClientId = Context.ClientMgmt.Store.Get().First().Id,
                Name = name + "(Updated)",
                AudienceType = audience.AudienceType,
                AudienceKey = audience.AudienceKey,
                Enabled = true
            };

            var result = await controller.UpdateAudience(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<AudienceResult>().Subject;

            data.Name.Should().Equals(model.Name);
        }
    }
}
