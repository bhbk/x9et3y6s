﻿using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using Bhbk.WebApi.Identity.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Admin.Tests.Controllers
{
    [Collection("AdminTestCollection")]
    public class UserControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly AdminClient _endpoints;

        public UserControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new AdminClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Admin_UserV1_AddClaim_Success()
        {

        }

        [Fact]
        public async Task Admin_UserV1_AddPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new UserController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

                var remove = await _factory.UoW.UserMgr.RemovePasswordAsync(user);
                remove.Should().BeAssignableTo(typeof(IdentityResult));
                remove.Succeeded.Should().BeTrue();

                var model = new UserAddPassword()
                {
                    NewPassword = RandomValues.CreateBase64String(16),
                    NewPasswordConfirm = RandomValues.CreateBase64String(16)
                };

                controller.SetUser(user.Id);

                var result = await controller.AddPasswordV1(user.Id, model) as BadRequestObjectResult;
                result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

                var check = await _factory.UoW.UserMgr.HasPasswordAsync(user);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_AddPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new UserController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

                var remove = await _factory.UoW.UserMgr.RemovePasswordAsync(user);
                remove.Should().BeAssignableTo(typeof(IdentityResult));
                remove.Succeeded.Should().BeTrue();

                var model = new UserAddPassword()
                {
                    NewPassword = Strings.ApiUnitTestUserPassNew,
                    NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
                };

                controller.SetUser(user.Id);

                var result = await controller.AddPasswordV1(user.Id, model) as NoContentResult;
                result.Should().BeAssignableTo(typeof(NoContentResult));

                var check = await _factory.UoW.UserMgr.CheckPasswordAsync(user, model.NewPassword);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Create_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.UserCreateV1(access.token, new UserCreate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            response = await _endpoints.UserCreateV1(access.token, new UserCreate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_Create_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var create = new UserCreate()
            {
                Email = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestUser1,
                FirstName = "First-" + RandomValues.CreateBase64String(4),
                LastName = "Last-" + RandomValues.CreateBase64String(4),
                PhoneNumber = RandomValues.CreateNumberAsString(10),
                LockoutEnabled = false,
                HumanBeing = true,
            };

            var response = await _endpoints.UserCreateV1NoConfirm(access.token, create);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<UserResult>();

            create = new UserCreate()
            {
                IssuerId = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single().Id,
                Email = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestUser1,
                FirstName = "First-" + RandomValues.CreateBase64String(4),
                LastName = "Last-" + RandomValues.CreateBase64String(4),
                PhoneNumber = RandomValues.CreateNumberAsString(10),
                LockoutEnabled = false,
                HumanBeing = false,
            };

            response = await _endpoints.UserCreateV1(access.token, create);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            check = ok.ToObject<UserResult>();
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.UserDeleteV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            response = await _endpoints.UserDeleteV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var model = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
            model.Immutable = true;

            await _factory.UoW.UserMgr.UpdateAsync(model);

            response = await _endpoints.UserDeleteV1(access.token, model.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_UserV1_Delete_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var model = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.UserDeleteV1(access.token, model.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = (await _factory.UoW.UserMgr.GetAsync(x => x.Id == model.Id)).Any();
            check.Should().BeFalse();
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Admin_UserV1_DeleteClaim_Success()
        {

        }

        [Fact]
        public async Task Admin_UserV1_Get_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
            var model = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser2)).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.UserGetV1(access.token, model.Id.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<UserResult>();

            response = await _endpoints.UserGetV1(access.token, model.Email);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            check = ok.ToObject<UserResult>();
        }

        [Fact]
        public async Task Admin_UserV1_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();
            await _factory.TestData.CreateRandomAsync(3);

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var take = 3;
            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("email", "asc"));

            var response = await _endpoints.UserGetPagesV1(access.token,
                new TuplePager()
                {
                    Filter = string.Empty,
                    Orders = orders,
                    Skip = 1,
                    Take = take,
                });

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var data = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<UserResult>>();
            var total = (int)ok["count"];

            data.Should().BeAssignableTo<IEnumerable<UserResult>>();
            data.Count().Should().Be(take);
            total.Should().Be(await _factory.UoW.UserMgr.Count());
        }

        [Fact(Skip = "NotImplemented")]
        public async Task Admin_UserV1_GetListClaims_Success()
        {

        }

        [Fact]
        public async Task Admin_UserV1_GetListClients_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new UserController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

                var result = await controller.GetUserClientsV1(user.Id) as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var list = ok.Value.Should().BeAssignableTo<IEnumerable<ClientResult>>().Subject;

                list.Count().Should().Be((await _factory.UoW.UserMgr.GetClientsAsync(user)).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetListLogins_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new UserController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

                var result = await controller.GetUserLoginsV1(user.Id) as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var list = ok.Value.Should().BeAssignableTo<IEnumerable<LoginResult>>().Subject;

                list.Count().Should().Be((await _factory.UoW.UserMgr.GetLoginsAsync(user)).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_GetListRoles_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new UserController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

                var result = await controller.GetUserRolesV1(user.Id) as OkObjectResult;
                var ok = result.Should().BeOfType<OkObjectResult>().Subject;
                var list = ok.Value.Should().BeAssignableTo<IEnumerable<RoleResult>>().Subject;

                list.Count().Should().Be((await _factory.UoW.UserMgr.GetRolesResultIdAsync(user)).Count());
            }
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.UserRemovePasswordV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            response = await _endpoints.UserRemovePasswordV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Admin_UserV1_RemovePassword_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();
            var model = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);
            var response = await _endpoints.UserRemovePasswordV1(access.token, model.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = await _factory.UoW.UserMgr.HasPasswordAsync(model);
            check.Should().BeFalse();
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new UserController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
                var model = new UserAddPassword()
                {
                    NewPassword = RandomValues.CreateBase64String(16),
                    NewPasswordConfirm = RandomValues.CreateBase64String(16)
                };

                controller.SetUser(user.Id);

                var result = await controller.SetPasswordV1(user.Id, model) as BadRequestObjectResult;
                result.Should().BeAssignableTo(typeof(BadRequestObjectResult));

                var check = await _factory.UoW.UserMgr.CheckPasswordAsync(user, model.NewPassword);
                check.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Admin_UserV1_SetPassword_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var controller = new UserController();
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

                var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
                var model = new UserAddPassword()
                {
                    NewPassword = Strings.ApiUnitTestUserPassNew,
                    NewPasswordConfirm = Strings.ApiUnitTestUserPassNew
                };

                controller.SetUser(user.Id);

                var result = await controller.SetPasswordV1(user.Id, model) as NoContentResult;
                result.Should().BeAssignableTo(typeof(NoContentResult));

                var check = await _factory.UoW.UserMgr.CheckPasswordAsync(user, model.NewPassword);
                check.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Admin_UserV1_Update_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var access = await JwtSecureProvider.CreateAccessTokenV2(_factory.UoW, issuer, new List<AppClient> { client }, user);

            var model = (await _factory.UoW.UserMgr.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
            model.FirstName += "(Updated)";

            var response = await _endpoints.UserUpdateV1(access.token, _factory.UoW.Convert.Map<UserUpdate>(model));

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<UserResult>();

            check.FirstName.Should().Be(model.FirstName);
        }
    }
}
