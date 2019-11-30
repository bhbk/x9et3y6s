using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Me.Tests.ServiceTests
{
    public class InfoServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly BaseServiceTests _factory;

        public InfoServiceTests(BaseServiceTests factory) => _factory = factory;

        [Fact]
        public async Task Me_InfoV1_GetMOTD_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var service = new MeService(conf, InstanceContext.UnitTest, owin);

                new TestData(uow, mapper).CreateMOTD(3);

                var result = await service.Info_GetMOTDV1();
                result.Should().BeAssignableTo<MOTDType1Model>();
            }
        }

        [Fact]
        public async Task Me_InfoV1_UpdateCode_Fail()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new MeService(conf, InstanceContext.UnitTest, owin);

                var dc = await service.Http.Info_UpdateCodeV1(Base64.CreateString(8), AlphaNumeric.CreateString(32), ActionType.Allow.ToString());
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                dc = await service.Http.Info_UpdateCodeV1(rop.RawData, AlphaNumeric.CreateString(32), ActionType.Allow.ToString());
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new MeService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultNormalUser).Single();

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingAccessExpire).Single();

                var state = uow.States.Create(
                    mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = client.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                    }));

                uow.Commit();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                var dc = await service.Http.Info_UpdateCodeV1(rop.RawData, state.StateValue, AlphaNumeric.CreateString(8));
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Me_InfoV1_UpdateCode_Success()
        {
            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new MeService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingAccessExpire).Single();

                var state = uow.States.Create(
                    mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = client.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                    }));

                uow.Commit();

                var dc = await service.Info_UpdateCodeV1(state.StateValue, ActionType.Allow.ToString());
                dc.Should().BeTrue();
            }

            using (var owin = _factory.CreateClient())
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                var auth = scope.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
                var service = new MeService(conf, InstanceContext.UnitTest, owin);

                var issuer = uow.Issuers.Get(x => x.Name == RealConstants.ApiDefaultIssuer).Single();
                var client = uow.Audiences.Get(x => x.Name == RealConstants.ApiDefaultClientUi).Single();
                var user = uow.Users.Get(x => x.Email == RealConstants.ApiDefaultNormalUser).Single();

                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                service.Jwt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenants:Salt"], new List<string>() { client.Name }, rop_claims);

                var expire = uow.Settings.Get(x => x.IssuerId == issuer.Id && x.AudienceId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingAccessExpire).Single();

                var state = uow.States.Create(
                    mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = client.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                    }));

                uow.Commit();

                var dc = await service.Info_UpdateCodeV1(state.StateValue, ActionType.Deny.ToString());
                dc.Should().BeTrue();
            }
        }
    }
}
