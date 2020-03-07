using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class ClaimRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Claims_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Claims.Create(new uvw_Claims());
            });
        }

        [Fact]
        public void Repo_Claims_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer).ToLambda())
                .Single();

            var result = UoW.Claims.Create(
                new uvw_Claims()
                {
                    IssuerId = issuer.Id,
                    Subject = Constants.ApiTestClaimSubject,
                    Type = Constants.ApiTestClaim,
                    Value = AlphaNumeric.CreateString(8),
                    ValueType = Constants.ApiTestClaimValueType,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<uvw_Claims>();
        }

        [Fact]
        public void Repo_Claims_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Claims.Delete(new uvw_Claims());
            });
        }

        [Fact]
        public void Repo_Claims_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<uvw_Claims>()
                .Where(x => x.Type == Constants.ApiTestClaim).ToLambda())
                .Single();

            UoW.Claims.Delete(claim);
        }

        [Fact]
        public void Repo_Claims_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Claims.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Claims>>();
            results.Count().Should().Be(UoW.Claims.Count());
        }

        [Fact]
        public void Repo_Claims_UpdateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Claims.Update(new uvw_Claims());
            });
        }

        [Fact]
        public void Repo_Claims_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<uvw_Claims>()
                .Where(x => x.Type == Constants.ApiTestClaim).ToLambda())
                .Single();
            claim.Value += "(Updated)";

            var result = UoW.Claims.Update(claim);
            result.Should().BeAssignableTo<uvw_Claims>();
        }
    }
}
