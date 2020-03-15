﻿using Bhbk.Lib.Cryptography.Entropy;
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
    public class RefreshRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Refreshes_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Refreshes.Create(
                    Mapper.Map<tbl_Refreshes>(new RefreshV1()));
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Refreshes_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer).ToLambda())
                .Single();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audiences>()
                .Where(x => x.Name == Constants.ApiTestAudience).ToLambda())
                .Single();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                .Where(x => x.UserName == Constants.ApiTestUser).ToLambda())
                .Single();

            var result = UoW.Refreshes.Create(
                Mapper.Map<tbl_Refreshes>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    AudienceId = audience.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = Base64.CreateString(8),
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));
            result.Should().BeAssignableTo<tbl_Refreshes>();

            UoW.Commit();
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Refreshes.Delete(new tbl_Refreshes());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var refresh = UoW.Refreshes.Get().First();

            UoW.Refreshes.Delete(refresh);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Refreshes_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Refreshes.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Refreshes>>();
            results.Count().Should().Be(UoW.Refreshes.Count());
        }

        [Fact]
        public void Repo_Refreshes_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.Refreshes.Update(new tbl_Refreshes());
                UoW.Commit();
            });
        }
    }
}