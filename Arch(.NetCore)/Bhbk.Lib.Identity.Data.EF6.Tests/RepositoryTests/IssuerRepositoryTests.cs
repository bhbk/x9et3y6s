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
    public class IssuerRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Issuers_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Issuers.Create(new uvw_Issuers());
            });
        }

        [Fact]
        public void Repo_Issuers_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var result = UoW.Issuers.Create(
                new uvw_Issuers()
                {
                    Name = Constants.ApiTestIssuer,
                    IssuerKey = Constants.ApiTestIssuerKey,
                    Enabled = true,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<uvw_Issuers>();
        }

        [Fact]
        public void Repo_Issuers_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Issuers.Delete(new uvw_Issuers());
            });
        }

        [Fact]
        public void Repo_Issuers_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(new QueryExpression<uvw_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer).ToLambda())
                .Single();

            UoW.Issuers.Delete(issuer);
        }

        [Fact]
        public void Repo_Issuers_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Issuers.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Issuers>>();
            results.Count().Should().Be(UoW.Issuers.Count());
        }

        [Fact]
        public void Repo_Issuers_UpdateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Issuers.Update(new uvw_Issuers());
            });
        }

        [Fact]
        public void Repo_Issuers_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(new QueryExpression<uvw_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer).ToLambda())
                .Single();
            issuer.Name += "(Updated)";

            var result = UoW.Issuers.Update(issuer);
            result.Should().BeAssignableTo<uvw_Issuers>();
        }
    }
}
