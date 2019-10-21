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
    public class ActivityRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Activities.Create(
                    Mapper.Map<uvw_Activities>(new ActivityCreate()));

                UoW.Commit();
            });
        }

        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_CreateV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var user = UoW.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

            var result = UoW.Activities.Create(
                Mapper.Map<uvw_Activities>(new ActivityCreate()
                {
                    ClientId = Guid.NewGuid(),
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    Immutable = false,
                }));
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
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var activity = UoW.Activities.Get(new QueryExpression<uvw_Activities>()
                .Where(x => x.Immutable == false).ToLambda());

            UoW.Activities.Delete(activity);
            UoW.Commit();
        }

        [Fact(Skip = "NotImplemented")]
        public void Repo_Activities_GetV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var results = UoW.Activities.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Activities>>();
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
