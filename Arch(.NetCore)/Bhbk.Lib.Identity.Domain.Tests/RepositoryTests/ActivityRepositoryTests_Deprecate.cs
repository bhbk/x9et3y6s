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
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class ActivityRepositoryTests_Deprecate : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_CreateV1_Fail_Deprecate()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Activities_Deprecate.Create(
                    Mapper.Map<tbl_Activities>(new ActivityCreate()));

                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Activities_CreateV1_Success_Deprecate()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var user = UoW.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

            var result = UoW.Activities_Deprecate.Create(
                Mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    ClientId = Guid.NewGuid(),
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    Immutable = false,
                }));
            result.Should().BeAssignableTo<tbl_Activities>();

            UoW.Commit();
        }

        [Fact]
        public void Repo_Activities_DeleteV1_Fail_Deprecate()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Activities_Deprecate.Delete(new tbl_Activities());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Activities_DeleteV1_Success_Deprecate()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var activity = UoW.Activities_Deprecate.Get(new QueryExpression<tbl_Activities>()
                .Where(x => x.Immutable == false).ToLambda());

            UoW.Activities_Deprecate.Delete(activity);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Activities_GetV1_Success_Deprecate()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var results = UoW.Activities_Deprecate.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Activities>>();
        }

        [Fact]
        public void Repo_Activities_UpdateV1_Fail_Deprecate()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.Activities_Deprecate.Update(new tbl_Activities());
                UoW.Commit();
            });
        }
    }
}
