using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT
{
    [Collection("RepositoryTests_DIRECT")]
    public class ActivityRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Activities.Delete(new tbl_Activity());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Activities_CreateV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .Single();

            var result = UoW.Activities.Create(
                Mapper.Map<tbl_Activity>(new ActivityV1()
                {
                    AudienceId = audience.Id,
                    ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                    IsDeletable = false,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Activity>();
        }

        [Fact]
        public void Repo_Activities_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Activities.Delete(new tbl_Activity());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Activities_DeleteV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var activity = UoW.Activities.Get(QueryExpressionFactory.GetQueryExpression<tbl_Activity>()
                .Where(x => x.IsDeletable == false).ToLambda());

            UoW.Activities.Delete(activity);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Activities_GetV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var results = UoW.Activities.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Activity>>();
            results.Count().Should().Be(UoW.Activities.Count());
        }

        [Fact]
        public void Repo_Activities_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.Activities.Update(new tbl_Activity());
                UoW.Commit();
            });
        }
    }
}
