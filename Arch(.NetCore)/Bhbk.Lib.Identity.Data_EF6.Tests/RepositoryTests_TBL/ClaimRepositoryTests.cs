using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data_EF6.Models_Tbl;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data_EF6.Tests.RepositoryTests_Tbl
{
    [Collection("RepositoryTests_Tbl")]
    public class ClaimRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Claims_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Claims.Create(new tbl_Claim());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateClaims();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == TestDefaultConstants.IssuerName).ToLambda())
                .Single();

            var result = UoW.Claims.Create(
                Mapper.Map<tbl_Claim>(new ClaimV1()
                {
                    IssuerId = issuer.Id,
                    Subject = TestDefaultConstants.ClaimSubject,
                    Type = TestDefaultConstants.ClaimName,
                    Value = AlphaNumeric.CreateString(8),
                    ValueType = TestDefaultConstants.ClaimValueType,
                    IsDeletable = true,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Claim>();
        }

        [Fact]
        public void Repo_Claims_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Claims.Delete(new tbl_Claim());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateClaims();

            var claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Type == TestDefaultConstants.ClaimName).ToLambda())
                .Single();

            UoW.Claims.Delete(claim);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Claims_GetV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateClaims();

            var results = UoW.Claims.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Claim>>();
            results.Count().Should().Be(UoW.Claims.Count());
        }

        [Fact]
        public void Repo_Claims_UpdateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Claims.Update(new tbl_Claim());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateClaims();

            var claim = UoW.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Type == TestDefaultConstants.ClaimName).ToLambda())
                .Single();
            claim.Value += "(Updated)";

            var result = UoW.Claims.Update(claim);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Claim>();
        }
    }
}
