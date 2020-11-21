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
    public class AudienceRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Audiences_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Audiences.Create(new tbl_Audience());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_CreateV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == Constants.TestIssuer).ToLambda())
                .Single();

            var result = UoW.Audiences.Create(
                Mapper.Map<tbl_Audience>(new AudienceV1()
                {
                    IssuerId = issuer.Id,
                    Name = Constants.TestAudience,
                    IsEnabled = true,
                    IsDeletable = true,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Audience>();
        }

        [Fact]
        public void Repo_Audiences_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Audiences.Delete(new tbl_Audience());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_DeleteV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .Single();

            UoW.Activities.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Activity>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            UoW.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            UoW.Settings.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            UoW.States.Delete(QueryExpressionFactory.GetQueryExpression<tbl_State>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());

            UoW.Roles.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.AudienceId == audience.Id).ToLambda());
            
            UoW.Audiences.Delete(audience);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Audiences_GetV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

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
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .Single();
            audience.Name += "(Updated)";

            var result = UoW.Audiences.Update(audience);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Audience>();
        }
    }
}
