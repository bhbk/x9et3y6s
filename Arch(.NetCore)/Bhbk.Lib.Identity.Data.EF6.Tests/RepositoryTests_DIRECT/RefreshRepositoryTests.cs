﻿using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
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

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests_DIRECT
{
    [Collection("RepositoryTests_DIRECT")]
    public class RefreshRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Refreshes_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Refreshes.Create(new tbl_Refresh());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Refreshes_CreateV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == Constants.TestIssuer).ToLambda())
                .Single();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .Single();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .Single();

            var result = UoW.Refreshes.Create(
                Mapper.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    AudienceId = audience.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = Base64.CreateString(8),
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Refresh>();
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Refreshes.Delete(new tbl_Refresh());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var activity = UoW.Refreshes.Get().First();

            UoW.Refreshes.Delete(activity);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Refreshes_GetV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var results = UoW.Refreshes.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Refresh>>();
            results.Count().Should().Be(UoW.Refreshes.Count());
        }

        [Fact]
        public void Repo_Refreshes_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.Refreshes.Update(new tbl_Refresh());
                UoW.Commit();
            });
        }
    }
}
