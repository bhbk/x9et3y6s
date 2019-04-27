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
    public class ActivityRepositoryTests
    {
        private StartupTests _factory;

        public ActivityRepositoryTests(StartupTests factory) => _factory = factory;

        [Fact(Skip = "NotImplemented")]
        public async Task Lib_ActivityRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _factory.UoW.ActivityRepo.CreateAsync(new ActivityCreate());
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _factory.UoW.ActivityRepo.CreateAsync(
                    new ActivityCreate()
                    {
                        ClientId = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false,
                    });
            });
        }

        [Fact]
        public async Task Lib_ActivityRepo_CreateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).First();

            var result = await _factory.UoW.ActivityRepo.CreateAsync(
                new ActivityCreate()
                {
                    ClientId = Guid.NewGuid(),
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    Immutable = false,
                });
            result.Should().BeAssignableTo<tbl_Activities>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ActivityRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.ActivityRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_ActivityRepo_DeleteV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var activity = (await _factory.UoW.ActivityRepo.GetAsync()).First();

            var result = await _factory.UoW.ActivityRepo.DeleteAsync(activity.Id);
            result.Should().BeTrue();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ActivityRepo_GetV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var result = await _factory.UoW.ActivityRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Activities>>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ActivityRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<NotImplementedException>(async () =>
            {
                await _factory.UoW.ActivityRepo.UpdateAsync(new tbl_Activities());
            });
        }
    }
}
