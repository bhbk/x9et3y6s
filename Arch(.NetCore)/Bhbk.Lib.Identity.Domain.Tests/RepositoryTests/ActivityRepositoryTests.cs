using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("LibraryRepositoryTests")]
    public class ActivityRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async ValueTask Repo_Activities_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.Activities.CreateAsync(
                    Mapper.Map<tbl_Activities>(new ActivityCreate()));

                await UoW.CommitAsync();
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.Activities.CreateAsync(
                    Mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        ClientId = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Activities_CreateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var user = (await UoW.Users.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            var result = await UoW.Activities.CreateAsync(
                Mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    ClientId = Guid.NewGuid(),
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    Immutable = false,
                }));
            result.Should().BeAssignableTo<tbl_Activities>();

            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Activities_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await UoW.Activities.DeleteAsync(new tbl_Activities());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Activities_DeleteV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var activity = (await UoW.Activities.GetAsync(new QueryExpression<tbl_Activities>()
                .Where(x => x.Immutable == false).ToLambda()));

            await UoW.Activities.DeleteAsync(activity);
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Activities_GetV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var result = await UoW.Activities.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Activities>>();

            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Activities_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<NotImplementedException>(async () =>
            {
                await UoW.Activities.UpdateAsync(new tbl_Activities());
                await UoW.CommitAsync();
            });
        }
    }
}
