using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.Tests.RepositoryTests_Tbl
{
    [Collection("RepositoryTests_Tbl")]
    public class AudienceRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Audiences_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Audiences.Create(new tbl_Audience());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateAudiences();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == TestDefaultConstants.IssuerName).ToLambda())
                .Single();

            var result = UoW.Audiences.Create(
                Mapper.Map<tbl_Audience>(new AudienceV1()
                    {
                        IssuerId = issuer.Id,
                        Name = TestDefaultConstants.AudienceName,
                        IsLockedOut = false,
                        IsDeletable = true,
                    }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Audience>();
        }

        [Fact]
        public void Repo_Audiences_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Audiences.Delete(new tbl_Audience());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateAudiences();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == TestDefaultConstants.AudienceName).ToLambda())
                .Single();

            UoW.Audiences.Delete(audience);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Audiences_GetV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateAudiences();

            var results = UoW.Audiences.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Audience>>();
            results.Count().Should().Be(UoW.Audiences.Count());
        }

        [Fact]
        public void Repo_Audiences_UpdateV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Audiences.Update(new tbl_Audience());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateAudiences();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == TestDefaultConstants.AudienceName).ToLambda())
                .Single();
            audience.Name += "(Updated)";

            var result = UoW.Audiences.Update(audience);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Audience>();
        }
    }
}
