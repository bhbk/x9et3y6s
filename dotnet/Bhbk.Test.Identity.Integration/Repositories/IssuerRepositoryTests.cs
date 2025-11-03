using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Builders;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Repositories
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
            var builder = new IssuerBuilder().WithDefaults();

            var result = UoW.Issuers.Post(builder.Build());
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Issuer>();

            UoW.Issuers.Delete(result);
            UoW.Commit();
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
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .Seed();

            var issuer = seed.Issuer;

            UoW.Claims.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            UoW.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            UoW.Settings.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            UoW.States.Delete(QueryExpressionFactory.GetQueryExpression<tbl_State>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            UoW.Roles.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Audience.IssuerId == issuer.Id).ToLambda());

            UoW.Audiences.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.IssuerId == issuer.Id).ToLambda());

            UoW.Issuers.Delete(issuer);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Issuers_GetV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .Seed();

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
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .Seed();

            var issuer = seed.Issuer;
            issuer.Name += "(Updated)";

            var result = UoW.Issuers.Put(issuer);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Issuer>();
        }
    }
}
