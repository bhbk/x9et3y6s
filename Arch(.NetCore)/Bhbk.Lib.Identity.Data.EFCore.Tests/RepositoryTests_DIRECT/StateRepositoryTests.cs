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
    public class StateRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_States_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.States.Create(
                    Mapper.Map<tbl_States>(new StateV1()));
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_States_CreateV1_Success()
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

            var result = UoW.States.Create(
                Mapper.Map<tbl_States>(
                    new StateV1()
                    {
                        IssuerId = issuer.Id,
                        AudienceId = audience.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));
            result.Should().BeAssignableTo<tbl_States>();

            UoW.Commit();
        }

        [Fact]
        public void Repo_States_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.States.Delete(new tbl_States());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_States_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var state = UoW.States.Get().First();

            UoW.States.Delete(state);
            UoW.Commit();
        }

        [Fact]
        public void Repo_States_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.States.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_States>>();
            results.Count().Should().Be(UoW.States.Count());
        }

        [Fact]
        public void Repo_States_UpdateV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.States.Update(new tbl_States());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_States_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                .Where(x => x.UserName == Constants.ApiTestUser).ToLambda())
                .Single();

            var state = UoW.States.Get(QueryExpressionFactory.GetQueryExpression<tbl_States>()
                .Where(x => x.UserId == user.Id).ToLambda())
                .First();

            state.StateConsume = true;

            var result = UoW.States.Update(state);
            result.Should().BeAssignableTo<tbl_States>();

            UoW.Commit();
        }
    }
}