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
    public class LoginProviderRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_LoginProviders_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.LoginProviders.Post(new tbl_LoginProvider());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_LoginProviders_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateLoginProviders();

            var result = UoW.LoginProviders.Post(
                Mapper.Map<tbl_LoginProvider>(new LoginProviderV1()
                {
                    Name = TestData.LoginProvider.Name,
                    ProviderKey = TestData.LoginProvider.ProviderKey,
                    IsDeletable = false,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_LoginProvider>();
        }

        [Fact]
        public void Repo_LoginProviders_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.LoginProviders.Delete(new tbl_LoginProvider());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_LoginProviders_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateLoginProviders();

            var loginProvider = UoW.LoginProviders.Get(QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>()
                .Where(x => x.Name == TestData.LoginProvider.Name).ToLambda())
                .Single();

            UoW.LoginProviders.Delete(loginProvider);
            UoW.Commit();
        }

        [Fact]
        public void Repo_LoginProviders_GetV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateLoginProviders();

            var results = UoW.LoginProviders.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_LoginProvider>>();
            results.Count().Should().Be(UoW.LoginProviders.Count());
        }

        [Fact]
        public void Repo_LoginProviders_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.LoginProviders.Put(new tbl_LoginProvider());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_LoginProviders_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateLoginProviders();

            var loginProvider = UoW.LoginProviders.Get(QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>()
                .Where(x => x.Name == TestData.LoginProvider.Name).ToLambda())
                .Single();
            loginProvider.Name += "(Updated)";

            var result = UoW.LoginProviders.Put(loginProvider);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_LoginProvider>();
        }
    }
}
