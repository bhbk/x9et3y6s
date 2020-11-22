using Bhbk.Lib.Identity.Data.EF6.Models_TSQL;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests_TSQL
{
    [Collection("RepositoryTests_TSQL")]
    public class ActivityRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Activities_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Activities.Create(new uvw_Activity());
                UoW.Commit();
            });
        }
        
        [Fact]
        public void Repo_Activities_CreateV1_Success()
        {
            var data = new TestDataFactory_TSQL(UoW);
            data.Destroy();
            data.CreateAudiences();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .Single();

            var result = UoW.Activities.Create(
                Mapper.Map<uvw_Activity>(new ActivityV1()
                {
                    AudienceId = audience.Id,
                    ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                    IsDeletable = true,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_Activity>();
        }

        [Fact]
        public void Repo_Activities_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Activities.Delete(new uvw_Activity());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Activities_DeleteV1_Success()
        {
            var data = new TestDataFactory_TSQL(UoW);
            data.Destroy();
            data.CreateAudiences();

            var activity = UoW.Activities.Get(QueryExpressionFactory.GetQueryExpression<uvw_Activity>()
                .Where(x => x.IsDeletable == true).ToLambda())
                .First();

            UoW.Activities.Delete(activity);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Activities_GetV1_Success()
        {
            var data = new TestDataFactory_TSQL(UoW);
            data.Destroy();
            data.CreateAudiences();

            var results = UoW.Activities.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Activity>>();
            results.Count().Should().Be(UoW.Activities.Count());
        }

        [Fact]
        public void Repo_Activities_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.Activities.Update(new uvw_Activity());
                UoW.Commit();
            });
        }
    }
}
