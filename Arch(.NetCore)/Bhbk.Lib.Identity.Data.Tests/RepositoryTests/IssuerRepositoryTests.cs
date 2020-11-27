using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class IssuerRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Issuers_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Issuers.Create(new uvw_Issuer());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Issuers_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateIssuers();

            var result = UoW.Issuers.Create(
                Mapper.Map<uvw_Issuer>(new IssuerV1()
                {
                    Name = Constants.TestIssuer,
                    IssuerKey = Constants.TestIssuerKey,
                    IsEnabled = true,
                    IsDeletable = false,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_Issuer>();
        }

        [Fact]
        public void Repo_Issuers_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Issuers.Delete(new uvw_Issuer());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Issuers_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateIssuers();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name == Constants.TestIssuer).ToLambda())
                .Single();

            UoW.Issuers.Delete(issuer);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Issuers_GetV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateIssuers();

            var results = UoW.Issuers.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Issuer>>();
            results.Count().Should().Be(UoW.Issuers.Count());
        }

        [Fact]
        public void Repo_Issuers_UpdateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Issuers.Update(new uvw_Issuer());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Issuers_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateIssuers();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name == Constants.TestIssuer).ToLambda())
                .Single();
            issuer.Name += "(Updated)";

            var result = UoW.Issuers.Update(issuer);
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_Issuer>();
        }
    }
}
