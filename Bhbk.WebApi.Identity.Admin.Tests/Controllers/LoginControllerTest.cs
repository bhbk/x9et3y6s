using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [TestClass]
    public class LoginControllerTest : StartupTest
    {
        private TestServer _owin;

        public LoginControllerTest()
        {
            _owin = new TestServer(new WebHostBuilder()
                .UseStartup<StartupTest>());
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_AddToUser_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new LoginController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();
            var login = new LoginCreate()
            {
                LoginProvider = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin1
            };

            var create = await _uow.LoginRepo.CreateAsync(_uow.Convert.Map<AppLogin>(login));

            await _uow.CommitAsync();

            var model = new UserLoginCreate()
            {
                UserId = user.Id,
                LoginId = create.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.LoginProvider,
                ProviderKey = Strings.ApiUnitTestLogin1Key,
                Enabled = true,
            };

            controller.SetUser(user.Id);

            var result = await controller.AddLoginToUserV1(model.LoginId, model.UserId, model) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_Delete_Fail_Immutable()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new LoginController(_conf, _uow, _tasks);
            var login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            login.Immutable = true;
            await _uow.LoginRepo.UpdateAsync(login);
            await _uow.CommitAsync();

            var result = await controller.DeleteLoginV1(login.Id) as BadRequestObjectResult;
            result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

            var check = (await _uow.LoginRepo.GetAsync(x => x.Id == login.Id)).Any();
            check.Should().BeTrue();
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_Create_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new LoginController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new LoginCreate()
            {
                LoginProvider = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin1
            };

            var result = await controller.CreateLoginV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.LoginProvider.Should().Be(model.LoginProvider);
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_RemoveFromUser_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new LoginController(_conf, _uow, _tasks);
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var login = await _uow.LoginRepo.CreateAsync(
                _uow.Convert.Map<AppLogin>(
                    new LoginCreate()
                    {
                        LoginProvider = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin1
                    }));

            await _uow.CommitAsync();

            var model = new UserLoginCreate()
            {
                UserId = user.Id,
                LoginId = login.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.LoginProvider,
                ProviderKey = Strings.ApiUnitTestLogin1Key,
                Enabled = true,
                Immutable = false
            };

            var add = await _uow.CustomUserMgr.AddLoginAsync(user,
                new UserLoginInfo(model.LoginProvider, model.ProviderKey, model.ProviderDisplayName));

            await _uow.CommitAsync();

            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            var result = await controller.RemoveLoginFromUserV1(model.LoginId, model.UserId) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_GetById_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new LoginController(_conf, _uow, _tasks);
            var login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();

            var result = await controller.GetLoginV1(login.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.Id.Should().Be(login.Id);
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_GetByName_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new LoginController(_conf, _uow, _tasks);
            var login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();

            var result = await controller.GetLoginV1(login.LoginProvider) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.Id.Should().Be(login.Id);
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_GetList_Fail_Auth()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var controller = new LoginController(_conf, _uow, _tasks);
            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", RandomValues.CreateBase64String(32));
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "loginprovider";
            ushort take = 3;
            ushort skip = 1;

            var response = await request.GetAsync("/login/v1?"
                + "orderBy=" + orderBy + "&"
                + "take=" + take.ToString() + "&"
                + "skip=" + skip.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_GetList_Fail_ParamInvalid()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var clients = new List<AppClient>();
            clients.Add(client);

            var access = JwtSecureProvider.CreateAccessTokenV2(_uow, issuer, clients, user).Result;

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string order = "loginprovider";

            var response = await request.GetAsync("/login/v1?"
                + "orderBy=" + order);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_GetList_Success()
        {
            _tests.Destroy();
            _tests.CreateRandom(10);
            _defaults.Create();

            var issuer = (await _uow.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _uow.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiDefaultUserAdmin).Single();

            var clients = new List<AppClient>();
            clients.Add(client);

            var access = JwtSecureProvider.CreateAccessTokenV2(_uow, issuer, clients, user).Result;

            var request = _owin.CreateClient();
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access.token);
            request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string orderBy = "loginprovider";
            ushort take = 3;
            ushort skip = 1;

            var response = await request.GetAsync("/login/v1?"
                + "orderBy=" + orderBy + "&"
                + "take=" + take.ToString() + "&"
                + "skip=" + skip.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JArray.Parse(await response.Content.ReadAsStringAsync()).ToObject<IEnumerable<LoginResult>>();
            var data = ok.Should().BeAssignableTo<IEnumerable<LoginResult>>().Subject;

            data.Count().Should().Be(take);
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_GetUserList_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new LoginController(_conf, _uow, _tasks);
            var login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();

            var result = await controller.GetLoginUsersV1(login.Id) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<IEnumerable<UserResult>>().Subject;

            data.Count().Should().Be((await _uow.LoginRepo.GetUsersAsync(login.Id)).Count());
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_Update_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new LoginController(_conf, _uow, _tasks);
            var login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var model = new LoginUpdate()
            {
                Id = login.Id,
                LoginProvider = login.LoginProvider + "(Updated)"
            };

            var result = await controller.UpdateLoginV1(model) as OkObjectResult;
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value.Should().BeAssignableTo<LoginResult>().Subject;

            data.LoginProvider.Should().Be(model.LoginProvider);
        }

        [TestMethod]
        public async Task Api_Admin_LoginV1_Delete_Success()
        {
            _tests.Destroy();
            _tests.Create();

            var controller = new LoginController(_conf, _uow, _tasks);
            var login = (await _uow.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();
            var user = _uow.CustomUserMgr.Store.Get(x => x.Email == Strings.ApiUnitTestUser1).Single();

            controller.SetUser(user.Id);

            var result = await controller.DeleteLoginV1(login.Id) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));

            var check = (await _uow.LoginRepo.GetAsync(x => x.Id == login.Id)).Any();
            check.Should().BeFalse();
        }
    }
}
