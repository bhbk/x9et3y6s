﻿using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    [Collection("AdminTests")]
    public class ActivityServiceTests
    {
        private readonly StartupTests _factory;

        public ActivityServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Admin_ActivityV1_Get_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();

                /*
                 * check model and/or action...
                 */

                var sts = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);

                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("created", "desc"));

                var result = await sts.HttpClient.Activity_GetV1(rop.token,
                    new CascadePager()
                    {
                        Filter = string.Empty,
                        Orders = orders,
                    });

                result.Should().BeAssignableTo(typeof(HttpResponseMessage));
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await _factory.TestData.DestroyAsync();
            }
        }

        [Fact]
        public async Task Admin_ActivityV1_Get_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                await _factory.TestData.CreateAsync();
                await _factory.TestData.CreateRandomAsync(3);

                var admin = new AdminService(_factory.Conf, _factory.UoW.InstanceType, owin);
                var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiDefaultIssuer)).Single();
                var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiDefaultClientUi)).Single();
                var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiDefaultAdminUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(_factory.UoW, issuer, new List<tbl_Clients> { client }, user);
                admin.Jwt = new JwtSecurityToken(rop.token);

                var take = 3;
                var orders = new List<Tuple<string, string>>();
                orders.Add(new Tuple<string, string>("created", "asc"));

                for (int i = 0; i <= take; i++)
                    await _factory.UoW.ActivityRepo.CreateAsync(new ActivityCreate()
                    {
                        UserId = user.Id,
                        ActivityType = "ActivityTest-" + RandomValues.CreateBase64String(32),
                        Immutable = false
                    });

                await _factory.UoW.CommitAsync();

                var result = admin.Activity_GetV1(new CascadePager()
                {
                    Filter = string.Empty,
                    Orders = orders,
                    Skip = 1,
                    Take = take,
                });

                result.Item1.Should().Be(await _factory.UoW.ActivityRepo.CountAsync());
                result.Item2.Should().BeAssignableTo<IEnumerable<ActivityModel>>();
                result.Item2.Count().Should().Be(take);

                await _factory.TestData.DestroyAsync();
            }
        }
    }
}