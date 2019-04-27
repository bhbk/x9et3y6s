using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.RepositoryTests
{
    [Collection("LibraryTests")]
    public class StateRepositoryTests
    {
        private StartupTests _factory;

        public StateRepositoryTests(StartupTests factory) => _factory = factory;

        [Fact(Skip = "NotImplemented")]
        public async Task Lib_StateRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _factory.UoW.StateRepo.CreateAsync(new StateCreate());
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _factory.UoW.StateRepo.CreateAsync(
                    new StateCreate()
                    {
                        IssuerId = Guid.NewGuid(),
                        ClientId = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        StateValue = RandomValues.CreateBase64String(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    });
            });
        }

        [Fact]
        public async Task Lib_StateRepo_CreateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).First();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).First();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).First();

            var result = await _factory.UoW.StateRepo.CreateAsync(
                new StateCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    StateValue = RandomValues.CreateBase64String(32),
                    StateType = StateType.Device.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                });
            result.Should().BeAssignableTo<tbl_States>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_StateRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.StateRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_StateRepo_DeleteV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var state = (await _factory.UoW.StateRepo.GetAsync()).First();

            var result = await _factory.UoW.StateRepo.DeleteAsync(state.Id);
            result.Should().BeTrue();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_StateRepo_GetV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var result = await _factory.UoW.StateRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_States>>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_StateRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.StateRepo.UpdateAsync(new tbl_States());
            });
        }
    }
}
