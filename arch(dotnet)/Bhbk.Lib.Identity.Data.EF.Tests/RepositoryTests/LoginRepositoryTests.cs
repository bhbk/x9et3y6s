using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class LoginRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Logins_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Logins.Create(new tbl_Login());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateLogins();

            var result = UoW.Logins.Create(
                Mapper.Map<tbl_Login>(new LoginV1()
                {
                    Name = TestData.Login.Name,
                    LoginKey = TestData.Login.LoginKey,
                    IsDeletable = false,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Login>();
        }

        [Fact]
        public void Repo_Logins_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Logins.Delete(new tbl_Login());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateLogins();

            var login = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name == TestData.Login.Name).ToLambda())
                .Single();

            UoW.Logins.Delete(login);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Logins_GetV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateLogins();

            var results = UoW.Logins.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Login>>();
            results.Count().Should().Be(UoW.Logins.Count());
        }

        [Fact]
        public void Repo_Logins_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Logins.Update(new tbl_Login());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateLogins();

            var login = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name == TestData.Login.Name).ToLambda())
                .Single();
            login.Name += "(Updated)";

            var result = UoW.Logins.Update(login);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Login>();
        }
    }
}
