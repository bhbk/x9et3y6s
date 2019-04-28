using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Helpers;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Tests.Helpers;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
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

namespace Bhbk.WebApi.Identity.Me.Tests.ServiceTests
{
    [Collection("MeTests")]
    public class InfoServiceTests
    {
        private readonly StartupTests _factory;

        public InfoServiceTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Me_InfoV1_GetQOTD_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new MeService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var result = service.Info_GetQOTDV1();
                result.Should().BeAssignableTo<QuotesModel>();
            }
        }

        [Fact]
        public async Task Me_InfoV1_UpdateCode_Fail()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new MeService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);

                var state = await uow.StateRepo.CreateAsync(
                    new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DeviceCodeTokenExpire),
                    });

                await uow.CommitAsync();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var dc = await service.Http.Info_UpdateCodeV1(RandomValues.CreateBase64String(32), state.StateValue, ActionType.Allow.ToString());
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                dc = await service.Http.Info_UpdateCodeV1(rop.token, RandomValues.CreateBase64String(32), ActionType.Allow.ToString());
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.NotFound);

                dc = await service.Http.Info_UpdateCodeV1(rop.token, state.StateValue, RandomValues.CreateAlphaNumericString(8));
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Me_InfoV1_UpdateCode_Success()
        {
            using (var owin = _factory.CreateClient())
            {
                var conf = _factory.Server.Host.Services.GetRequiredService<IConfigurationRoot>();
                var uow = _factory.Server.Host.Services.GetRequiredService<IIdentityUnitOfWork>();
                var service = new MeService(conf, uow.InstanceType, owin);

                await new TestData(uow).DestroyAsync();
                await new TestData(uow).CreateAsync();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == Constants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == Constants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == Constants.ApiDefaultNormalUser)).Single();

                var salt = conf["IdentityTenants:Salt"];
                salt.Should().Be(uow.IssuerRepo.Salt);

                var secret = await new TotpHelper(8, 10).GenerateAsync(user.SecurityStamp, user);

                var state = await uow.StateRepo.CreateAsync(
                    new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(uow.ConfigRepo.DeviceCodeTokenExpire),
                    });

                await uow.CommitAsync();

                var rop = await JwtFactory.UserResourceOwnerV2(uow, issuer, new List<tbl_Clients> { client }, user);

                var dc = await service.Http.Info_UpdateCodeV1(rop.token, state.StateValue, ActionType.Allow.ToString());
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.NoContent);

                dc = await service.Http.Info_UpdateCodeV1(rop.token, state.StateValue, ActionType.Deny.ToString());
                dc.Should().BeAssignableTo(typeof(HttpResponseMessage));
                dc.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
