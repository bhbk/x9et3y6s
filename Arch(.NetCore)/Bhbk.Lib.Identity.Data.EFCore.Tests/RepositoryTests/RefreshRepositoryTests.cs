﻿using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Models.Admin;
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
    public class RefreshRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Refreshes_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Refreshes.Create(new uvw_Refresh());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Refreshes_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name == Constants.TestIssuer).ToLambda())
                .Single();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .Single();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .Single();

            var result = UoW.Refreshes.Create(
                Mapper.Map<uvw_Refresh>(new RefreshV1()
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

            result.Should().BeAssignableTo<uvw_Refresh>();
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Refreshes.Delete(new uvw_Refresh());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var activity = UoW.Refreshes.Get().First();

            UoW.Refreshes.Delete(activity);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Refreshes_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Refreshes.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Refresh>>();
            results.Count().Should().Be(UoW.Refreshes.Count());
        }

        [Fact]
        public void Repo_Refreshes_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.Refreshes.Update(new uvw_Refresh());
                UoW.Commit();
            });
        }
    }
}
