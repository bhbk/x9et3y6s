using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Primitives;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests
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

            var issuer = UoW.Issuers.Get(new QueryExpression<uvw_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer).ToLambda())
                .Single();

            var result = UoW.Claims.Create(
                new uvw_Claims()
                {
                    IssuerId = issuer.Id,
                    Subject = "Subject-" + AlphaNumeric.CreateString(4),
                    Type = Constants.ApiTestClaim,
                    Value = AlphaNumeric.CreateString(8),
                    ValueType = "ValueType-" + AlphaNumeric.CreateString(4),
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

            var claim = UoW.Claims.Get(new QueryExpression<uvw_Claims>()
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

            var claim = UoW.Claims.Get(new QueryExpression<uvw_Claims>()
                .Where(x => x.Type == Constants.ApiTestClaim).ToLambda())
                .Single();
            claim.Value += "(Updated)";

            var result = UoW.Claims.Update(claim);
            result.Should().BeAssignableTo<uvw_Claims>();
        }
    }
}
