using Bhbk.Lib.Identity.Data_EF6.Models;
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

namespace Bhbk.Lib.Identity.Data_EF6.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class IssuerRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Issuers_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Issuers.Create(new E_Issuer());
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
                Mapper.Map<E_Issuer>(new IssuerV1()
                {
                    Name = TestDefaultConstants.IssuerName,
                    IssuerKey = TestDefaultConstants.IssuerKey,
                    IsEnabled = true,
                    IsDeletable = true,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<E_Issuer>();
        }

        [Fact]
        public void Repo_Issuers_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Issuers.Delete(new E_Issuer());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Issuers_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateIssuers();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<E_Issuer>()
                .Where(x => x.Name == TestDefaultConstants.IssuerName).ToLambda())
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
            results.Should().BeAssignableTo<IEnumerable<E_Issuer>>();
            results.Count().Should().Be(UoW.Issuers.Count());
        }

        [Fact]
        public void Repo_Issuers_UpdateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Issuers.Update(new E_Issuer());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Issuers_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateIssuers();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<E_Issuer>()
                .Where(x => x.Name == TestDefaultConstants.IssuerName).ToLambda())
                .Single();
            issuer.Name += "(Updated)";

            var result = UoW.Issuers.Update(issuer);
            UoW.Commit();

            result.Should().BeAssignableTo<E_Issuer>();
        }
    }
}
