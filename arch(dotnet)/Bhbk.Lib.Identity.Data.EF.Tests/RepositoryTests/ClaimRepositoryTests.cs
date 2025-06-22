using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class ClaimRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Claims_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Claims.Create(new tbl_Claim());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateClaims();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == TestData.Issuer.Name).ToLambda())
                .Single();

            var result = UoW.Claims.Create(
                Mapper.Map<tbl_Claim>(new ClaimV1()
                {
                    IssuerId = issuer.Id,
                    Subject = TestData.Claim.Subject,
                    Type = TestData.Claim.Name,
                    Value = AlphaNumeric.CreateString(8),
                    ValueType = TestData.Claim.ValueType,
                    IsDeletable = false,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Claim>();
        }

        [Fact]
        public void Repo_Claims_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Claims.Delete(new tbl_Claim());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateClaims();

            var claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Type == TestData.Claim.Name).ToLambda())
                .Single();

            UoW.Claims.Delete(claim);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Claims_GetV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateClaims();

            var results = UoW.Claims.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Claim>>();
            results.Count().Should().Be(UoW.Claims.Count());
        }

        [Fact]
        public void Repo_Claims_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Claims.Update(new tbl_Claim());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateClaims();

            var claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Type == TestData.Claim.Name).ToLambda())
                .Single();
            claim.Value += "(Updated)";

            var result = UoW.Claims.Update(claim);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Claim>();
        }
    }
}
