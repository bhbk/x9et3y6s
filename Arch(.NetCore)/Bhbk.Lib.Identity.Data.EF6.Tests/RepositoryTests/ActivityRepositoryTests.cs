using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class ActivityRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Activities.Create(
                    Mapper.Map<tbl_Activities>(new ActivityCreate()));

                UoW.Commit();
            });
        }

        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var result = UoW.Activities.Create(
                Mapper.Map<tbl_Activities>(new ActivityCreate()
                {
                    AudienceId = Guid.NewGuid(),
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    Immutable = false,
                }));
            result.Should().BeAssignableTo<tbl_Activities>();

            UoW.Commit();
        }

        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Activities.Delete(new tbl_Activities());
                UoW.Commit();
            });
        }

        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var activity = UoW.Activities.Get(new QueryExpression<tbl_Activities>()
                .Where(x => x.Immutable == false).ToLambda());

            UoW.Activities.Delete(activity);
            UoW.Commit();
        }

        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Activities.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Activities>>();
            results.Count().Should().Be(UoW.Activities.Count());
        }

        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.Activities.Update(new tbl_Activities());
                UoW.Commit();
            });
        }
    }
}
