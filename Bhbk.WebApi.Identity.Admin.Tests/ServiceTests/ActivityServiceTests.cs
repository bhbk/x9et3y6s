using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Domain.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;
using static Bhbk.Lib.DataState.Models.DataPagerV3;
using RealConstants = Bhbk.Lib.Identity.Data.Primitives.Constants;

namespace Bhbk.WebApi.Identity.Admin.Tests.ServiceTests
{
    public class ActivityServiceTests : IClassFixture<BaseServiceTests>
    {
        private readonly IConfiguration _conf;
        private readonly IMapper _mapper;
        private readonly BaseServiceTests _factory;
        private readonly AdminService _service;

        public ActivityServiceTests(BaseServiceTests factory)
        {
            _factory = factory;

            var http = _factory.CreateClient();

            _conf = _factory.Server.Host.Services.GetRequiredService<IConfiguration>();
            _mapper = _factory.Server.Host.Services.GetRequiredService<IMapper>();
            _service = new AdminService(_conf, InstanceContext.UnitTest, http);
        }

        [Fact]
        public async Task Admin_ActivityV1_Get_Success()
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync().Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                _service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                var testActivity = (await uow.ActivityRepo.GetAsync()).First();

                var result = _service.Activity_GetV1(testActivity.Id.ToString());
                result.Should().BeAssignableTo<ActivityModel>();
            }

            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                new TestData(uow, _mapper).CreateAsync(3).Wait();

                var issuer = (await uow.IssuerRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultIssuer)).Single();
                var client = (await uow.ClientRepo.GetAsync(x => x.Name == RealConstants.ApiDefaultClientUi)).Single();
                var user = (await uow.UserRepo.GetAsync(x => x.Email == RealConstants.ApiDefaultAdminUser)).Single();

                _service.Jwt = await JwtFactory.UserResourceOwnerV2(uow, _mapper, issuer, new List<tbl_Clients> { client }, user);

                int take = 2;
                var state = new DataPagerV3()
                {
                    Filter = new List<FilterDescriptor>()
                    {
                        new FilterDescriptor(){ Field = string.Empty, Value = string.Empty }
                    },
                    Sort = new List<SortDescriptor>() 
                    {
                        new SortDescriptor() { Field = "created", Dir = "asc" }
                    },
                    Skip = 0,
                    Take = take
                };

                var result = _service.Activity_GetV1(state);
                result.Data.ToDynamicList().Count().Should().Be(take);
                result.Total.Should().Be(await uow.ActivityRepo.CountAsync());
            }
        }
    }
}
