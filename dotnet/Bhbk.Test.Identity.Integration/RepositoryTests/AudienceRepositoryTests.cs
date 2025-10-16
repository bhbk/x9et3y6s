using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class AudienceRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Audiences_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Audiences.Post(new tbl_Audience());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateAudiences();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == TestData.Issuer.Name).ToLambda())
                .Single();

            var result = UoW.Audiences.Post(
                Mapper.Map<tbl_Audience>(new AudienceV1()
                    {
                        IssuerId = issuer.Id,
                        Name = TestData.Audience.Name,
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
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateAudiences();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == TestData.Audience.Name).ToLambda())
                .Single();

            UoW.Audiences.Delete(audience);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Audiences_GetV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
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
                UoW.Audiences.Put(new tbl_Audience());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateAudiences();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == TestData.Audience.Name).ToLambda())
                .Single();
            audience.Name += "(Updated)";

            UoW.Audiences.Put(audience);
            UoW.Commit();

            audience.Should().BeAssignableTo<tbl_Audience>();
        }
    }
}
