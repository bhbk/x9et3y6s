using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class IssuerRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Issuers_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Issuers.Post(new tbl_Issuer());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Issuers_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateIssuers();

            var issuerEntity = Mapper.Map<tbl_Issuer>(new IssuerV1()
                    {
                        Name = TestData.Issuer.Name,
                        IsEnabled = true,
                        IsDeletable = false,
                    });
            issuerEntity.IssuerKey = TestData.Issuer.IssuerKey;

            var result = UoW.Issuers.Post(issuerEntity);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Issuer>();
        }

        [Fact]
        public void Repo_Issuers_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Issuers.Delete(new tbl_Issuer());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Issuers_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateIssuers();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == TestData.Issuer.Name).ToLambda())
                .Single();

            var claims = UoW.Claims.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            var refreshes = UoW.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            var settings = UoW.Settings.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            var states = UoW.States.Delete(QueryExpressionFactory.GetQueryExpression<tbl_State>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            var roles = UoW.Roles.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Audience.IssuerId == issuer.Id).ToLambda());

            var audiences = UoW.Audiences.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            UoW.Issuers.Delete(issuer);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Issuers_GetV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateIssuers();

            var results = UoW.Issuers.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Issuer>>();
            results.Count().Should().Be(UoW.Issuers.Count());
        }

        [Fact]
        public void Repo_Issuers_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Issuers.Put(new tbl_Issuer());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Issuers_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateIssuers();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == TestData.Issuer.Name).ToLambda())
                .Single();
            issuer.Name += "(Updated)";

            var result = UoW.Issuers.Put(issuer);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Issuer>();
        }
    }
}
