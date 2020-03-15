﻿using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
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
    public class ClaimRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Claims_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Claims.Create(
                    Mapper.Map<tbl_Claims>(new ClaimV1()));

                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer).ToLambda())
                .Single();

            var result = UoW.Claims.Create(
                Mapper.Map<tbl_Claims>(new ClaimV1()
                {
                    IssuerId = issuer.Id,
                    Subject = Constants.ApiTestClaimSubject,
                    Type = Constants.ApiTestClaim,
                    Value = AlphaNumeric.CreateString(8),
                    ValueType = Constants.ApiTestClaimValueType,
                    Immutable = false,
                }));
            result.Should().BeAssignableTo<tbl_Claims>();

            UoW.Commit();
        }

        [Fact]
        public void Repo_Claims_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Claims.Delete(new tbl_Claims());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claims>()
                .Where(x => x.Type == Constants.ApiTestClaim).ToLambda())
                .Single();

            UoW.Claims.Delete(claim);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Claims_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Claims.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Claims>>();
            results.Count().Should().Be(UoW.Claims.Count());
        }

        [Fact]
        public void Repo_Claims_UpdateV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Claims.Update(new tbl_Claims());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claims>()
                .Where(x => x.Type == Constants.ApiTestClaim).ToLambda())
                .Single();
            claim.Value += "(Updated)";

            var result = UoW.Claims.Update(claim);
            result.Should().BeAssignableTo<tbl_Claims>();

            UoW.Commit();
        }
    }
}