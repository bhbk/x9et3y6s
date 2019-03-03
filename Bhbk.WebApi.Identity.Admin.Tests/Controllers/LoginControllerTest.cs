﻿using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Providers;
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
    public class LoginControllerTest
    {
        private readonly StartupTest _factory;
        private readonly HttpClient _client;
        private readonly AdminClient _endpoints;

        public LoginControllerTest(StartupTest factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _endpoints = new AdminClient(_factory.Conf, _factory.UoW.Situation, _client);
        }

        [Fact]
        public async Task Admin_LoginV1_AddToUser_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var controller = new LoginController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
            var login = new LoginCreate()
            {
                LoginProvider = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin1
            };

            var create = await _factory.UoW.LoginRepo.CreateAsync(login);

            await _factory.UoW.CommitAsync();

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

        [Fact]
        public async Task Admin_LoginV1_Create_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.Login_CreateV1(access.token, new LoginCreate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            response = await _endpoints.Login_CreateV1(access.token, new LoginCreate());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_LoginV1_Create_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);

            var create = new LoginCreate()
            {
                LoginProvider = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin1
            };

            var response = await _endpoints.Login_CreateV1(access.token, create);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<LoginModel>();
        }

        [Fact]
        public async Task Admin_LoginV1_Delete_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.Login_DeleteV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            response = await _endpoints.Login_DeleteV1(access.token, Guid.NewGuid());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Admin_LoginV1_Delete_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();
            var model = (await _factory.UoW.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.Login_DeleteV1(access.token, model.Id);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var check = (await _factory.UoW.LoginRepo.GetAsync(x => x.Id == model.Id)).Any();
            check.Should().BeFalse();
        }

        [Fact]
        public async Task Admin_LoginV1_Get_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();
            var model = (await _factory.UoW.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.Login_GetV1(access.token, model.Id.ToString());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<LoginModel>();

            response = await _endpoints.Login_GetV1(access.token, model.LoginProvider);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            check = ok.ToObject<LoginModel>();
        }

        [Fact]
        public async Task Admin_LoginV1_GetList_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();
            await _factory.TestData.CreateRandomAsync(3);

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);

            var take = 3;
            var orders = new List<Tuple<string, string>>();
            orders.Add(new Tuple<string, string>("loginprovider", "asc"));

            var response = await _endpoints.Login_GetV1(access.token,
                new CascadePager()
                {
                    Filter = string.Empty,
                    Orders = orders,
                    Skip = 1,
                    Take = take,
                });

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var list = JArray.Parse(ok["list"].ToString()).ToObject<IEnumerable<LoginModel>>();
            var count = (int)ok["count"];

            list.Should().BeAssignableTo<IEnumerable<LoginModel>>();
            list.Count().Should().Be(take);
            count.Should().Be(await _factory.UoW.IssuerRepo.Count());
        }

        [Fact]
        public async Task Admin_LoginV1_RemoveFromUser_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var controller = new LoginController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _factory.Server.Host.Services;

            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            controller.SetUser(user.Id);

            var login = await _factory.UoW.LoginRepo.CreateAsync(new LoginCreate()
            {
                LoginProvider = RandomValues.CreateBase64String(4) + "-" + Strings.ApiUnitTestLogin1
            });

            await _factory.UoW.CommitAsync();

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

            var add = await _factory.UoW.UserRepo.AddLoginAsync(user.Id,
                new UserLoginInfo(model.LoginProvider, model.ProviderKey, model.ProviderDisplayName));

            await _factory.UoW.CommitAsync();

            add.Should().BeAssignableTo(typeof(IdentityResult));
            add.Succeeded.Should().BeTrue();

            var result = await controller.RemoveLoginFromUserV1(model.LoginId, model.UserId) as NoContentResult;
            result.Should().BeAssignableTo(typeof(NoContentResult));
        }

        [Fact]
        public async Task Admin_LoginV1_Update_Fail()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            /*
             * check security...
             */

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer1)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient1)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser1)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            var response = await _endpoints.Login_UpdateV1(access.token, new LoginModel());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            /*
             * check model and/or action...
             */

            issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);
            response = await _endpoints.Login_UpdateV1(access.token, new LoginModel());

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_LoginV1_Update_Success()
        {
            await _factory.TestData.DestroyAsync();
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultUserAdmin)).Single();

            var access = await JwtBuilder.CreateAccessTokenV2(_factory.UoW, issuer, new List<ClientModel> { client }, user);

            var model = (await _factory.UoW.LoginRepo.GetAsync(x => x.LoginProvider == Strings.ApiUnitTestLogin1)).Single();
            model.LoginProvider += "(Updated)";

            var response = await _endpoints.Login_UpdateV1(access.token, model);

            response.Should().BeAssignableTo(typeof(HttpResponseMessage));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var ok = JObject.Parse(await response.Content.ReadAsStringAsync());
            var check = ok.ToObject<LoginModel>();

            check.LoginProvider.Should().Be(model.LoginProvider);
        }
    }
}
