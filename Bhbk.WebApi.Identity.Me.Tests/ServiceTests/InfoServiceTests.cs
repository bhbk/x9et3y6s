using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
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
        private readonly IConfiguration _conf;
        private readonly IMapper _mapper;
        private readonly BaseServiceTests _factory;
        private readonly MeService _service;

        public InfoServiceTests(BaseServiceTests factory)
        {
            _factory = factory;

            var http = _factory.CreateClient();

            _conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            _mapper = _factory.Server.Host.Services.GetRequiredService<IMapper>();
            _service = new MeService(_conf, InstanceContext.UnitTest, http);
        }

        [Fact]
        public void Me_InfoV1_GetMOTD_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateMOTDAsync(3).Wait();

                var result = _service.Info_GetMOTDV1();
                result.Should().BeAssignableTo<MOTDType1Model>();
            }
        }

        [Fact]
        public async Task Me_InfoV1_UpdateCode_Fail()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var dc = await _service.Http.Info_UpdateCodeV1(Base64.CreateString(8), AlphaNumeric.CreateString(32), ActionType.Allow.ToString());
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).Single();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                dc = await _service.Http.Info_UpdateCodeV1(rop.RawData, AlphaNumeric.CreateString(32), ActionType.Allow.ToString());
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).Single();

                var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingAccessExpire)).Single();

                var state = await uow.StateRepo.CreateAsync(
                    _mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = Base64.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                    }));

                uow.CommitAsync().Wait();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var dc = await _service.Http.Info_UpdateCodeV1(rop.RawData, state.StateValue, AlphaNumeric.CreateString(8));
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Me_InfoV1_UpdateCode_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).Single();

                _service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingAccessExpire)).Single();

                var state = await uow.StateRepo.CreateAsync(
                    _mapper.Map<tbl_States>(new StateCreate()
                        {
                            IssuerId = issuer.Id,
                            ClientId = client.Id,
                            UserId = user.Id,
                            StateValue = Base64.CreateString(32),
                            StateType = StateType.Device.ToString(),
                            StateConsume = false,
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                        }));

                uow.CommitAsync().Wait();

                var dc = _service.Info_UpdateCodeV1(state.StateValue, ActionType.Allow.ToString());
                dc.Should().BeTrue();
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultNormalUser)).Single();

                _service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var expire = (await uow.SettingRepo.GetAsync(x => x.IssuerId == issuer.Id && x.ClientId == null && x.UserId == null
                    && x.ConfigKey == RealConstants.ApiSettingAccessExpire)).Single();

                var state = await uow.StateRepo.CreateAsync(
                    _mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = Base64.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uint.Parse(expire.ConfigValue)),
                    }));

                uow.CommitAsync().Wait();

                var dc = _service.Info_UpdateCodeV1(state.StateValue, ActionType.Deny.ToString());
                dc.Should().BeTrue();
            }
        }
    }
}
