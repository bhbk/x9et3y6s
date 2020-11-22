using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Models_TSQL;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_TSQL
{
    [Collection("RepositoryTests_TSQL")]
    public class UserRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Users_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Users.Create(new uvw_User());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_CreateV1_Success()
        {
            var result = UoW.Users.Create(
                new uvw_User()
                {
                    UserName = Constants.TestUser,
                    EmailAddress = Constants.TestUser,
                    PhoneNumber = NumberAs.CreateString(9),
                    FirstName = "First-" + Base64.CreateString(4),
                    LastName = "Last-" + Base64.CreateString(4),
                    IsHumanBeing = true,
                    IsLockedOut = false,
                    IsDeletable = true,
                });
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_User>();
        }

        [Fact]
        public void Repo_Users_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Users.Delete(new uvw_User());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_DeleteV1_Success()
        {
            var data = new TestDataFactory_TSQL(UoW);
            data.Destroy();
            data.CreateUsers();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda()).Single();

            UoW.Users.Delete(user);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Users_GetV1_Success()
        {
            var data = new TestDataFactory_TSQL(UoW);
            data.Destroy();
            data.CreateUsers();

            var results = UoW.Users.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_User>>();
            results.Count().Should().Be(UoW.Users.Count());
        }

        [Fact]
        public void Repo_Users_UpdateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Users.Update(new uvw_User());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_UpdateV1_Success()
        {
            var data = new TestDataFactory_TSQL(UoW);
            data.Destroy();
            data.CreateUsers();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda()).Single();
            user.FirstName += "(Updated)";
            user.LastName += "(Updated)";

            var result = UoW.Users.Update(user);
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_User>();
        }
    }
}
