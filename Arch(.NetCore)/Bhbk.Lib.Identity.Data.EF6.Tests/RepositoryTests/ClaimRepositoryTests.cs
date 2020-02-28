using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Primitives;
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

            var issuer = UoW.Issuers.Get(new QueryExpression<uvw_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer).ToLambda())
                .Single();

            var result = UoW.Claims.Create(
                new uvw_Claims()
                {
                    IssuerId = issuer.Id,
                    Type = Constants.ApiTestClaim,
                    Value = Base64.CreateString(8),
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

            var audience = UoW.Claims.Get(new QueryExpression<uvw_Claims>()
                .Where(x => x.Type == Constants.ApiTestClaim).ToLambda())
                .Single();

            UoW.Claims.Delete(audience);
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

        [Fact(Skip = "NotImplemented")]
        public void Repo_Claims_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var audience = UoW.Claims.Get(new QueryExpression<uvw_Claims>()
                .Where(x => x.Type == Constants.ApiTestAudience).ToLambda())
                .Single();
            audience.Value += "(Updated)";

            var result = UoW.Claims.Update(audience);
            result.Should().BeAssignableTo<uvw_Claims>();
        }
    }
}
