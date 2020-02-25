using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests
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
            
            var audience = UoW.Context.Set<uvw_Audiences>()
                .Where(x => x.Name == Constants.ApiTestAudience)
                .Single();

            var result = UoW.Activities.Create(
                new uvw_Activities()
                {
                    Id = Guid.NewGuid(),
                    AudienceId = audience.Id,
                    ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                    Created = DateTime.Now,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<uvw_Activities>();

            UoW.Commit();
        }

        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Activities.Delete(new uvw_Activities());
                UoW.Commit();
            });
        }

        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var activity = UoW.Activities.Get(new QueryExpression<uvw_Activities>()
                .Where(x => x.Immutable == false).ToLambda());

            UoW.Activities.Delete(activity);
            UoW.Commit();
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

        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.Activities.Update(new uvw_Activities());
                UoW.Commit();
            });
        }
    }
}
