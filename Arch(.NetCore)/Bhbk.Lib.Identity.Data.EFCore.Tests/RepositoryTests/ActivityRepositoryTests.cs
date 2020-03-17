using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class ActivityRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Activities_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Activities.Create(new uvw_Activities());
            });
        }

        [Fact]
        public void Repo_Activities_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audiences>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .Single();

            var result = UoW.Activities.Create(
                new uvw_Activities()
                {
                    AudienceId = audience.Id,
                    ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                    Immutable = false,
                });
            result.Should().BeAssignableTo<uvw_Activities>();
        }

        [Fact]
        public void Repo_Activities_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Activities.Delete(new uvw_Activities());
            });
        }

        [Fact]
        public void Repo_Activities_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var activity = UoW.Activities.Get(QueryExpressionFactory.GetQueryExpression<uvw_Activities>()
                .Where(x => x.Immutable == false).ToLambda())
                .First();

            UoW.Activities.Delete(activity);
        }

        [Fact]
        public void Repo_Activities_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Activities.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Activities>>();
            results.Count().Should().Be(UoW.Activities.Count());
        }

        [Fact]
        public void Repo_Activities_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.Activities.Update(new uvw_Activities());
            });
        }
    }
}
