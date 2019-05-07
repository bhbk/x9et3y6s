using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("LibraryTestsCollection")]
    public class StateRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_StateRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.StateRepo.CreateAsync(
                    UoW.Mapper.Map<tbl_States>(new StateCreate()));
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.StateRepo.CreateAsync(
                    UoW.Mapper.Map<tbl_States>(new StateCreate()
                        {
                            IssuerId = Guid.NewGuid(),
                            ClientId = Guid.NewGuid(),
                            UserId = Guid.NewGuid(),
                            StateValue = RandomValues.CreateBase64String(32),
                            StateType = StateType.Device.ToString(),
                            StateConsume = false,
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                        }));
            });
        }

        [Fact]
        public async Task Lib_StateRepo_CreateV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var client = (await UoW.ClientRepo.GetAsync(x => x.Name == Constants.ApiTestClient)).Single();
            var user = (await UoW.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            var result = await UoW.StateRepo.CreateAsync(
                UoW.Mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));
            result.Should().BeAssignableTo<tbl_States>();
        }

        [Fact]
        public async Task Lib_StateRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.StateRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_StateRepo_DeleteV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var state = (await UoW.StateRepo.GetAsync()).First();

            var result = await UoW.StateRepo.DeleteAsync(state.Id);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Lib_StateRepo_GetV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var result = await UoW.StateRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_States>>();
        }

        [Fact]
        public async Task Lib_StateRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.StateRepo.UpdateAsync(new tbl_States());
            });
        }
    }
}
